﻿namespace FSCL.Runtime.CacheInspection

open Cloo
open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open FSCL.Compiler
open FSCL.Runtime

[<Step("FSCL_CACHE_INSPECTION_STEP", 
       Dependencies = [| "FSCL_MODULE_PARSING_STEP" |], 
       Before = [| "FSCL_MODULE_PREPROCESSING_STEP" |])>] 
type CacheInspectionStep(tm: TypeManager,
                          processors: ICompilerStepProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule>(tm, processors)

    override this.Run(kmodule) =
        if kmodule.CustomInfo.ContainsKey("RUNTIME_CACHE") then
            // Get cache
            let cache = kmodule.CustomInfo.["RUNTIME_CACHE"] :?> RuntimeCache
            // Skip kernels already compiled
            for k in kmodule.CallGraph.KernelIDs do
                if cache.Kernels.ContainsKey(k) then
                    let cachedKernel = cache.Kernels.[k]
                    let kernel = kmodule.CallGraph.GetKernel(k)
                    kernel.Skip <- true
                    kernel.Body <- cachedKernel.Info.Body
                    kernel.Codegen <- cachedKernel.Info.Codegen
                    for item in cachedKernel.Info.CustomInfo do
                        kernel.CustomInfo.Add(item.Key, item.Value)
                    for item in cachedKernel.Info.ParameterInfo do
                        kernel.ParameterInfo.Add(item.Key, item.Value)
                    kernel.Signature <- cachedKernel.Info.Signature
        kmodule
        
