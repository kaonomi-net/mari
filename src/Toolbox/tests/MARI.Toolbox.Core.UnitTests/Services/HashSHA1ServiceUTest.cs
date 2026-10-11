// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Services
{
    using MARI.Toolbox.Services;

    using System.Security.Cryptography;

    public class HashSHA1ServiceUTest : HashBaseServiceUTest<HashSHA1Service>
    {
        public HashSHA1ServiceUTest() 
            : base(SHA1.Create)
        {
        }
    }
}
