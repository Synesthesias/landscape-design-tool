using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Runtime
{
    public interface IResponseMessage
    {
        /// <summary>
        /// The message returned from the server.
        /// </summary>
        string Message { get; }
    }
}