using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
[Serializable] public class SaveEnvelope { public int version=1; public string hash,payload; }
}
