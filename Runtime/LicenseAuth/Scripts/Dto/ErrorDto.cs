using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime
{
    public class ErrorDto : IResponseMessage
    {
        public string Message { get; set; }

        public string Code { get; set; }
    }
}