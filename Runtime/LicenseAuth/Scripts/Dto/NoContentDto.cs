using System;

namespace Landscape2.Runtime.LicenseAuth
{
    /// <summary>
    /// APIのレスポンスが空のときに使うクラス
    /// </summary>
    /// <remarks>
    /// JsonUtility.FromJson<T> は空のクラスをデシリアライズできないので、空のクラスを作っておく
    /// </remarks>
    [Serializable]
    public class NoContentDto
    {
    }
}