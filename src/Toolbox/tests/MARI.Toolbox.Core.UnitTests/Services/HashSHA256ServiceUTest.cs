// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Services
{
    using MARI.Toolbox.Services;

    using System.Security.Cryptography;

    public class HashSHA256ServiceUTest : HashBaseServiceUTest<HashSHA256Service>
    {
        public HashSHA256ServiceUTest() 
            : base(SHA256.Create)
        {
        }
    }
}
