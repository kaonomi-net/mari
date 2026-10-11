// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.UnitTests.ToolKit.Helpers
{
    using AutoFixture;
    using AutoFixture.AutoNSubstitute;

    using MARI.Toolbox.Models;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using NFluent;

    using System.Diagnostics;

    /// <summary>
    /// Helper unit test to validate serialization
    /// </summary>
    public static class ObjectTestHelper
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ObjectTestHelper"/> class.
        /// </summary>
        static ObjectTestHelper()
        {
            SimpleTypes = new Type[]
            {
                typeof(int),
                typeof(long),
                typeof(float),
                typeof(double),
                typeof(string),
                typeof(char),
                typeof(byte),
                typeof(uint),
                typeof(ulong),
                typeof(Guid),
            };
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the scalar types.
        /// </summary>
        public static IReadOnlyList<Type> SimpleTypes { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Prepares the fixture.
        /// </summary>
        public static Fixture PrepareFixture(Fixture? fixture = null,
                                             bool supportMutableValueType = false,
                                             bool supportCyclingReference = false,
                                             bool supportServiceSubstitution = true)
        {
            fixture ??= new Fixture();

            if (supportMutableValueType)
            {
                var customization = new SupportMutableValueTypesCustomization();
                customization.Customize(fixture);
            }

            if (supportCyclingReference)
            {
                fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                                 .ForEach(b => fixture.Behaviors.Remove(b));

                fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            }

            if (supportServiceSubstitution)
            {
                var nsubstitute = new AutoNSubstituteCustomization();
                fixture.Customize(nsubstitute);
            }

            return fixture;
        }

        /// <summary>
        /// Ensures the surrogate is serialization and deserializable.
        /// </summary>
        public static bool IsSerializable<TType>(bool supportMutableValueType = false,
                                                 bool supportCyclingReference = false,
                                                 Func<Fixture, TType>? typeProvider = null,
                                                 Func<TType, TType, bool>? overrideComparer = null,
                                                 Fixture? manualFixture = null)
        {
            var shouldIndent = Debugger.IsAttached ? Formatting.Indented : Formatting.None;
            var fixture = PrepareFixture(manualFixture,
                                         supportMutableValueType: supportMutableValueType,
                                         supportCyclingReference: supportCyclingReference);

            TType data;
            if (typeProvider is not null)
                data = typeProvider(fixture);
            else
                data = fixture.Create<TType>();

            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = shouldIndent
            };

            var serializationErrors = new List<ErrorContext>();

            jsonSerializerSettings.Error += (source, args) => serializationErrors.Add(args.ErrorContext);

            Exception? serializationException = null;
            string jsonStr = string.Empty;
            try
            {
                jsonStr = JsonConvert.SerializeObject(data, jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                serializationException = ex;
            }

            var serializationErrorMsg = "Serialization : \n" + string.Join("\n", serializationErrors.Select(e => string.Format("Path: {0} -> Error {1}", e.Path, e.Error))) + "\n" + serializationException?.GetFullString();

            Check.WithCustomMessage(serializationErrorMsg)
                 .That(serializationException).IsNull();

            Check.WithCustomMessage(serializationErrorMsg)
                 .That(serializationErrors.Count).IsEqualTo(0);

            Check.That(jsonStr).Not.IsNullOrEmpty();

            Exception? deserializationException = null;
            TType? deserializedData = default;
            try
            {
                deserializedData = JsonConvert.DeserializeObject<TType>(jsonStr, jsonSerializerSettings);
            }
            catch (Exception ex)
            {
                deserializationException = ex;
            }

            var deserializationErrorMsg = "Deserialization : \n" + string.Join("\n", serializationErrors.Select(e => string.Format("Path: {0} -> Error {1}", e.Path, e.Error))) + "\n" + deserializationException?.GetFullString();

            Check.WithCustomMessage(deserializationErrorMsg)
                 .That(deserializationException).IsNull();

            Check.WithCustomMessage(deserializationErrorMsg)
                 .That(serializationErrors.Count).IsEqualTo(0);

            Check.WithCustomMessage("Test type is not null or default").That(deserializedData).IsNotEqualTo(default);

            if (overrideComparer != null)
            {
                var comparer = overrideComparer(data, deserializedData!);
                return comparer;
            }

            CheckThatObjectAreEquals(data, deserializedData, shouldIndent);

            return true;
        }

        /// <summary>
        /// Generates the random type of the abstract.
        /// </summary>
        /// <param name="sources">The sources of type to search if no source is provide then use the default basic types sources</param>
        public static AbstractType GenerateRandomAbstractType(params Type[] sources)
        {
            IReadOnlyList<Type> types = sources;
            if (types is null || types.Count == 0)
                types = SimpleTypes;

            var type = types[Random.Shared.Next(0, types.Count)];
            return type.GetAbstractType();
        }

        /// <summary>
        /// Simple object comparaison
        /// By default a json serialization comparaison is done
        /// </summary>
        /// <remarks>
        ///     To override to more deep quality
        /// </remarks>
        public static void CheckThatObjectAreEquals<TType>(TType data, TType deserializedData, Formatting shouldIndent)
        {
            var serializedDataJson = JsonConvert.SerializeObject(data, shouldIndent);
            var deserializedDataJson = JsonConvert.SerializeObject(deserializedData, shouldIndent);

            Check.That(serializedDataJson).IsEqualTo(deserializedDataJson);
        }

        #endregion
    }
}
