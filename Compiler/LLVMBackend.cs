using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Compiler.AST;

using LLVMSharp;

namespace Compiler
{
    class LLVMBackend
    {
        private Node tree;

        private LLVMModuleRef mod;

        private LLVMValueRef putchar;
        private LLVMValueRef getchar;
        private LLVMValueRef memset;

        private LLVMBuilderRef builder;
        private LLVMValueRef mainfn;
        private LLVMValueRef data;
        private LLVMValueRef ptr;

        public LLVMBackend(Node tree)
        {
            this.tree = tree;
        }

        public void Generate()
        {
            mod = LLVM.ModuleCreateWithName("bfout");

            // Main entry point
            var ft = LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[] { }, false);
            mainfn = LLVM.AddFunction(mod, "main", ft);

            var entry = LLVM.AppendBasicBlock(mainfn, "entry");
            builder = LLVM.CreateBuilder();
            LLVM.PositionBuilderAtEnd(builder, entry);

            // External functions for IO/memset intrinsic
            ft = LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[] { LLVM.Int32Type() }, false);
            putchar = LLVM.AddFunction(mod, "putchar", ft);
            LLVM.SetLinkage(putchar, LLVMLinkage.LLVMExternalLinkage);
            ft = LLVM.FunctionType(LLVM.Int32Type(), new LLVMTypeRef[] { }, false);
            getchar = LLVM.AddFunction(mod, "getchar", ft);
            LLVM.SetLinkage(getchar, LLVMLinkage.LLVMExternalLinkage);
            ft = LLVM.FunctionType(LLVM.VoidType(), new LLVMTypeRef[] { LLVM.PointerType(LLVM.Int8Type(), 0), LLVM.Int8Type(), LLVM.Int64Type(), LLVM.Int32Type(), LLVM.Int1Type() }, false);
            memset = LLVM.AddFunction(mod, "llvm.memset.p0i8.i64", ft);

            // Local variables (data array + ptr) on stack
            data = LLVM.BuildArrayAlloca(builder, LLVM.Int8Type(), LLVM.ConstInt(LLVM.Int64Type(), 100000, false), "data");
            ptr = LLVM.BuildAlloca(builder, LLVM.Int64Type(), "ptr");
            
            // Zero data + ptr
            LLVM.BuildStore(builder, LLVM.ConstInt(LLVM.Int64Type(), 0, false), ptr);
            LLVM.BuildCall(builder, memset, new LLVMValueRef[] { data, LLVM.ConstInt(LLVM.Int8Type(), 0, false), LLVM.ConstInt(LLVM.Int64Type(), 100000, false), LLVM.ConstInt(LLVM.Int32Type(), 0, false), LLVM.ConstInt(LLVM.Int1Type(), 0, false) }, "");
            
            Generate(tree);

            LLVM.BuildRet(builder, LLVM.ConstInt(LLVM.Int32Type(), 0, false));
            
            LLVM.VerifyModule(mod, LLVMVerifierFailureAction.LLVMPrintMessageAction, out var _);

            LLVM.DumpModule(mod);

            var pm = LLVM.PassManagerBuilderCreate();
            LLVM.PassManagerBuilderSetOptLevel(pm, 3);
            var pmr = LLVM.CreatePassManager();
            LLVM.PassManagerBuilderPopulateModulePassManager(pm, pmr);
            LLVM.RunPassManager(pmr, mod);

            LLVM.DumpModule(mod);
        }

        private void Generate(Node node)
        {
            if (node == null)
                return;

            switch(node)
            {
                case DataNode n:
                    GenerateData(n.Change);
                    break;
                case PtrNode n:
                    GeneratePtr(n.Change);
                    break;
                case InputNode n:
                    GenerateInput();
                    break;
                case OutputNode n:
                    GenerateOutput();
                    break;
                case LoopNode n:
                    GenerateLoop(n.Inner);
                    break;
            }

            Generate(node.Next);
        }

        private LLVMValueRef GetData()
        {
            var derefptr = LLVM.BuildLoad(builder, ptr, "getdata_ptr");
            var addr = LLVM.BuildGEP(builder, data, new LLVMValueRef[] { derefptr }, "getdata_addr");
            return LLVM.BuildLoad(builder, addr, "getdata_data");
        }

        private void SetData(LLVMValueRef val)
        {
            var derefptr = LLVM.BuildLoad(builder, ptr, "setdata_ptr");
            var addr = LLVM.BuildGEP(builder, data, new LLVMValueRef[] { derefptr }, "setdata_addr");
            LLVM.BuildStore(builder, val, addr);
        }

        private void GenerateData(int change)
        {
            var derefptr = LLVM.BuildLoad(builder, ptr, "gendata_ptr");
            var addr = LLVM.BuildGEP(builder, data, new LLVMValueRef[] { derefptr }, "gendata_addr");
            var tmp = LLVM.BuildLoad(builder, addr, "gendata_data");
            tmp = LLVM.BuildAdd(builder, tmp, LLVM.ConstInt(LLVM.Int8Type(), (ulong)change, false), "gendata_result");
            LLVM.BuildStore(builder, tmp, addr);
        }

        private void GeneratePtr(int change)
        {
            var derefptr = LLVM.BuildLoad(builder, ptr, "genptr_ptr");
            var tmp = LLVM.BuildAdd(builder, derefptr, LLVM.ConstInt(LLVM.Int64Type(), (ulong)change, false), "genptr_result");
            LLVM.BuildStore(builder, tmp, ptr);
        }

        private void GenerateOutput()
        {
            var tmp = GetData();
            tmp = LLVM.BuildZExt(builder, tmp, LLVM.Int32Type(), "genout_cast");
            LLVM.BuildCall(builder, putchar, new LLVMValueRef[] { tmp }, "genout_out");
        }

        private void GenerateInput()
        {
            var tmp = LLVM.BuildCall(builder, getchar, new LLVMValueRef[] { }, "genin_in");
            tmp = LLVM.BuildTrunc(builder, tmp, LLVM.Int8Type(), "genin_cast");
            SetData(tmp);
        }

        private void GenerateLoop(Node child)
        {
            var preloop = LLVM.AppendBasicBlock(mainfn, "genloop_pre");
            LLVM.BuildBr(builder, preloop);
            LLVM.PositionBuilderAtEnd(builder, preloop);
            var tmp = GetData();
            var cond = LLVM.BuildICmp(builder, LLVMIntPredicate.LLVMIntEQ, tmp, LLVM.ConstInt(LLVM.Int8Type(), 0, false), "genloop_cond");
            var loop = LLVM.AppendBasicBlock(mainfn, "genloop_loop");
            var postloop = LLVM.AppendBasicBlock(mainfn, "genloop_post");
            LLVM.BuildCondBr(builder, cond, postloop, loop);
            LLVM.PositionBuilderAtEnd(builder, loop);
            Generate(child);
            LLVM.BuildBr(builder, preloop);
            LLVM.PositionBuilderAtEnd(builder, postloop);
        }
    }
}
