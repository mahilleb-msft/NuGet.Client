using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Common.Telemetry
{
    /// <summary>
    /// Holds a Telemetry Pii Property Value
    /// Used to send a complex object [e.g. an array] containing PII property values to the underlying Telemetry provider
    /// </summary>
    public class TelemetryPiiPropertyValue
    {
        object _value = null;
        public TelemetryPiiPropertyValue(object value)
        {
            _value = value;
        }
    }
}
