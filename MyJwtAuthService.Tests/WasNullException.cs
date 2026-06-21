using System;
using System.Collections.Generic;
using System.Text;

namespace MyJwtAuthService.Tests
{
    public class WasNullException:Exception
    {
        public WasNullException(string variableName): base($"{variableName} was null") { }

        public static void ThrowIfNull(object? obj, string variableName)
        {
            if (obj is null)
            {
                throw new WasNullException(variableName);
            }
        }
    }
}
