// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Configuration
{
    using MARI.Toolbox.Disposables;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Primitives;

    using System.Collections.Generic;

    /// <summary>
    /// Configuration proxy used to transparently apply templates
    /// </summary>
    /// <seealso cref="IConfigurationProvider" />
    internal sealed class TemplatedConfigurationProxySourceProvider : SafeDisposable, IConfigurationProvider, IConfigurationSource
    {
        #region Fields

        private const string SharedTemplateHostName = "SharedTemplates";
        private const string TemplatePlaceHolderName = "Template";
        private const string Separator = ":";

        private readonly IConfigurationProvider _source;

        private IReadOnlyDictionary<string, IReadOnlyCollection<string>> _indexedTemplates;
        private IReadOnlyDictionary<string, string?> _templateValues;
        private HashSet<string> _sourceKeys;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatedConfigurationProxySourceProvider"/> class.
        /// </summary>
        public TemplatedConfigurationProxySourceProvider(IConfigurationProvider source)
        {
            this._indexedTemplates = new Dictionary<string, IReadOnlyCollection<string>>();
            this._templateValues = new Dictionary<string, string?>();
            this._sourceKeys = new HashSet<string>();

            this._source = source;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
        {
            var childKeys = this._source.GetChildKeys(earlierKeys, parentPath);

            if (!string.IsNullOrEmpty(parentPath))
            {
                var parentPrefix = parentPath + Separator;
                var missingKey = this._sourceKeys.Where(s => s.StartsWith(parentPrefix, StringComparison.OrdinalIgnoreCase) &&
                                                             !s.EndsWith(TemplatePlaceHolderName, StringComparison.OrdinalIgnoreCase))
                                                 .Select(s =>
                                                 {
                                                     var separatorIndx = s.IndexOf(Separator, Math.Min(s.Length, parentPath.Length + 1));

                                                     var parentEndIndx = parentPath.Length + 1;
                                                     var length = s.Length - parentEndIndx;

                                                     if (separatorIndx > -1)
                                                         length = separatorIndx - parentEndIndx;

                                                     if (length <= 0)
                                                         return string.Empty;

                                                     var subKey = s.Substring(parentEndIndx, length);

                                                     return subKey;
                                                 })
                                                 .Except(childKeys)
                                                 .Except(earlierKeys)
                                                 .Where(s => !string.IsNullOrEmpty(s))
                                                 .ToArray();

                return childKeys.Concat(missingKey)
                                .ToArray();
            }

            return childKeys;
        }

        /// <inheritdoc />
        public IChangeToken GetReloadToken()
        {
            return this._source.GetReloadToken();
        }

        /// <inheritdoc />
        public void Load()
        {
            this._source.Load();

            var sourceKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            BuildSourceKeysRec(sourceKeys, null);

            // Extract template information
            var sharedTemplatesHosts = sourceKeys.Where(k => k.EndsWith(SharedTemplateHostName, StringComparison.OrdinalIgnoreCase))
                                                 .Distinct()
                                                 .ToArray();

            var templates = new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase);
            var templateValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            var newKeyTemplateValues = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (var templateHost in sharedTemplatesHosts)
            {
                var templateNames = this._source.GetChildKeys(EnumerableHelper<string>.ReadOnly, templateHost);

                foreach (var templateName in templateNames.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    // Same template name declared in multiple SharedTemplates hosts : first one wins
                    if (templates.ContainsKey(templateName))
                        continue;

                    var templateFullKey = templateHost + Separator + templateName;

                    var templateKeys = new HashSet<string>();
                    BuildSourceKeysRec(templateKeys, templateFullKey);

                    var cleanedTemplateKeys = templateKeys.Select(k => (key: k, cleanKey: k.Replace(templateFullKey, "").Trim(':')))
                                                          .ToArray();

                    foreach (var kv in cleanedTemplateKeys)
                    {
                        var templateLocalKey = templateName + Separator + kv.cleanKey;

                        if (this._source.TryGet(kv.key, out var tplValue))
                            templateValues.Add(templateLocalKey, tplValue);
                    }

                    templates.Add(templateName, cleanedTemplateKeys.Select(kv => kv.cleanKey).ToArray());
                }
            }

            // Apply template values
            var placeHolders = sourceKeys.Where(k => k.EndsWith(TemplatePlaceHolderName, StringComparison.OrdinalIgnoreCase))
                                         .Distinct()
                                         .ToArray();

            foreach (var placeHolder in placeHolders)
            {
                var templateName = string.Empty;
                if (!this._source.TryGet(placeHolder, out templateName) || string.IsNullOrEmpty(templateName))
                    continue;

                var root = placeHolder.Substring(0, placeHolder.Length - TemplatePlaceHolderName.Length);

                if (templates.TryGetValue(templateName, out var templateKeys))
                {
                    foreach (var templateKey in templateKeys)
                    {
                        // Intermediate section keys have no value: only the key is registered (TryGet will return null for it)
                        if (sourceKeys.Add(root + templateKey) && templateValues.TryGetValue(templateName + Separator + templateKey, out var tplValue))
                            newKeyTemplateValues[root + templateKey] = tplValue;
                    }
                }
            }

            this._sourceKeys = sourceKeys;
            this._indexedTemplates = templates;
            this._templateValues = newKeyTemplateValues;
        }

        /// <inheritdoc />
        public void Set(string key, string? value)
        {
            this._source.Set(key, value);
            // TODO : invalid template cache
        }

        /// <inheritdoc />
        public bool TryGet(string key, out string? value)
        {
            if (this._source.TryGet(key, out value))
                return true;

            if (this._templateValues.TryGetValue(key, out value))
                return true;

            if (this._sourceKeys.TryGetValue(key, out value))
            {
                if (string.Equals(key, value))
                {
                    value = null;
                    return true;
                }
                return TryGet(value, out value);
            }

            return false;
        }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return this;
        }

        /// <inheritdoc />
        protected override void DisposeBegin()
        {
            if (this._source is IDisposable disposable)
                disposable.Dispose();

            base.DisposeBegin();
        }

        /// <summary>
        /// Builds the source keys record.
        /// </summary>
        /// <param name="earlierKeys">The earlier keys.</param>
        /// <param name="parentPath">The parent path.</param>
        private void BuildSourceKeysRec(HashSet<string> earlierKeys, string? parentPath)
        {
            var children = this._source.GetChildKeys(EnumerableHelper<string>.ReadOnly, parentPath);

            foreach (var childKey in children.Distinct())
            {
                var fullKey = (string.IsNullOrEmpty(parentPath)
                                                  ? string.Empty
                                                  : parentPath + Separator) + childKey;

                if (earlierKeys.Add(fullKey))
                    BuildSourceKeysRec(earlierKeys, fullKey);
            }
        }

        #endregion
    }
}
