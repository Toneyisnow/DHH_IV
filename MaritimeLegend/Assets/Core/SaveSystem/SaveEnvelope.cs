using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
[Serializable] public class SaveEnvelope { public int version=SaveContract.Version; public string hash,payload; }
}
