using Dapper;
using Microsoft.Data.SqlClient;
using EnvelopeGenerator.Core.Models;
using System.Text;
using System.Runtime.CompilerServices;

namespace EnvelopeGenerator.Core.Services;

[המשך הקוד הקודם]

private string GetTablePrefix(int source) => source switch
{
    1 => "sh",
    2 => "sl",
    3 => "shn",
    4 => "",
    5 => "shd",
    _ => throw new ArgumentException($"Unknown source: {source}")
};

[המשך המימוש...]