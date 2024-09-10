using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire.Hosting.AWS.Lambda;

public interface ILambdaFunctionMetadata : IProjectMetadata
{
    /// <summary>
    ///
    /// </summary>
    public string Handler { get; }

    /// <summary>
    ///
    /// </summary>
    public string? OutputPath { get; }

    /// <summary>
    ///
    /// </summary>
    public string[] Traits { get; }

    bool IsClassLibrary { get; }
}

public class LambdaFunctionMetadata : ILambdaFunctionMetadata, IProjectMetadata
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="path"></param>
    /// <param name="handler"></param>
    public LambdaFunctionMetadata(string path, string handler, bool isClassLibrary)
    {
        ProjectPath = path;
        Handler = handler;
        IsClassLibrary = isClassLibrary;
    }

    /// <summary>
    ///
    /// </summary>
    public string ProjectPath { get; }
    /// <summary>
    ///
    /// </summary>
    public string Handler { get; }
    /// <summary>
    ///
    /// </summary>
    public string? OutputPath => null;
    /// <summary>
    ///
    /// </summary>
    public string[] Traits => [];

    public bool IsClassLibrary { get; }
}