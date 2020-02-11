#include "wrapper_common.h"
#include "dss.h"

#if __cplusplus
extern "C" {
#endif

    // Notes: zero-based indexing is used for rowIdx[] and colPtr[]. 

    DLLEXPORT dss_int s_dss_solve(const dss_int matrixStructure, const dss_int matrixType, const dss_int systemType,
        const dss_int nRows, const dss_int nCols, const dss_int nnz, const dss_int rowIdx[], const dss_int colPtr[], const float values[],
        const dss_int nRhs, const float rhsValues[], float solValues[])
    {
        _MKL_DSS_HANDLE_t handle;
        dss_int error;

        dss_int opt = MKL_DSS_MSG_LVL_WARNING + MKL_DSS_TERM_LVL_ERROR + MKL_DSS_ZERO_BASED_INDEXING + MKL_DSS_AUTO_ORDER + MKL_DSS_SINGLE_PRECISION;
        if (systemType) opt += MKL_DSS_TRANSPOSE_SOLVE; // solve a transposed system, A'x = b
        
        // Initialize the solver
        error = dss_create(handle, opt);
        if (error != MKL_DSS_SUCCESS) return error;

        // Define the non-zero structure of the matrix
        dss_int sym = (matrixStructure == 0)
            ? MKL_DSS_SYMMETRIC_STRUCTURE
            : (matrixStructure == 1)
                ? MKL_DSS_SYMMETRIC
                : MKL_DSS_NON_SYMMETRIC;
        error = dss_define_structure(handle, sym, rowIdx, nRows, nCols, colPtr, nnz);
        if (error != MKL_DSS_SUCCESS) return error;

        // Reorder the matrix
        error = dss_reorder(handle, opt, 0);
        if (error != MKL_DSS_SUCCESS) return error;

        // Factor the matrix
        dss_int type = (matrixType == 0)
            ? MKL_DSS_POSITIVE_DEFINITE
            : MKL_DSS_INDEFINITE;
        error = dss_factor_real(handle, type, values);
        if (error != MKL_DSS_SUCCESS) return error;

        // Get the solution vector
        error = dss_solve_real(handle, opt, rhsValues, nRhs, solValues);
        if (error != MKL_DSS_SUCCESS) return error;

        // Deallocate solver storage
        error = dss_delete(handle, opt);
        return error;
    }    

    DLLEXPORT dss_int d_dss_solve(const dss_int matrixStructure, const dss_int matrixType, const dss_int systemType,
        const dss_int nRows, const dss_int nCols, const dss_int nnz, const dss_int rowIdx[], const dss_int colPtr[], const double values[],
        const dss_int nRhs, const double rhsValues[], double solValues[])
    {
        _MKL_DSS_HANDLE_t handle;
        dss_int error;

        dss_int opt = MKL_DSS_MSG_LVL_WARNING + MKL_DSS_TERM_LVL_ERROR + MKL_DSS_ZERO_BASED_INDEXING + MKL_DSS_AUTO_ORDER;
        if (systemType) opt += MKL_DSS_TRANSPOSE_SOLVE; // solve a transposed system, A'x = b

        // Initialize the solver
        error = dss_create(handle, opt);
        if (error != MKL_DSS_SUCCESS) return error;

        // Define the non-zero structure of the matrix
        dss_int sym = (matrixStructure == 0)
            ? MKL_DSS_SYMMETRIC_STRUCTURE
            : (matrixStructure == 1)
                ? MKL_DSS_SYMMETRIC
                : MKL_DSS_NON_SYMMETRIC;
        error = dss_define_structure(handle, sym, rowIdx, nRows, nCols, colPtr, nnz);
        if (error != MKL_DSS_SUCCESS) return error;

        // Reorder the matrix
        error = dss_reorder(handle, opt, 0);
        if (error != MKL_DSS_SUCCESS) return error;

        // Factor the matrix
        dss_int type = (matrixType == 0)
            ? MKL_DSS_POSITIVE_DEFINITE
            : MKL_DSS_INDEFINITE;
        error = dss_factor_real(handle, type, values);
        if (error != MKL_DSS_SUCCESS) return error;

        // Get the solution vector
        error = dss_solve_real(handle, opt, rhsValues, nRhs, solValues);
        if (error != MKL_DSS_SUCCESS) return error;

        // Deallocate solver storage
        error = dss_delete(handle, opt);
        return error;
    }

    DLLEXPORT dss_int c_dss_solve(const dss_int matrixStructure, const dss_int matrixType, const dss_int systemType,
        const dss_int nRows, const int nCols, const int nnz, const dss_int const rowIdx[], const dss_int colPtr[], const dss_complex_float values[],
        const dss_int nRhs, const dss_complex_float rhsValues[], dss_complex_float solValues[])
    {
        _MKL_DSS_HANDLE_t handle;
        dss_int error;

        dss_int opt = MKL_DSS_MSG_LVL_WARNING + MKL_DSS_TERM_LVL_ERROR + MKL_DSS_ZERO_BASED_INDEXING + MKL_DSS_AUTO_ORDER + MKL_DSS_SINGLE_PRECISION;
        if (systemType == 1) opt += MKL_DSS_CONJUGATE_SOLVE; // solve a conjugate transposed system, A¢Óx = b
        else if(systemType == 2) opt += MKL_DSS_TRANSPOSE_SOLVE; // solve a transposed system, A'x = b

        // Initialize the solver
        error = dss_create(handle, opt);
        if (error != MKL_DSS_SUCCESS) return error;

        // Define the non-zero structure of the matrix
        dss_int sym = (matrixStructure == 0)
            ? MKL_DSS_SYMMETRIC_STRUCTURE_COMPLEX
            : (matrixStructure == 1)
                ? MKL_DSS_SYMMETRIC_COMPLEX
                : MKL_DSS_NON_SYMMETRIC_COMPLEX;
        error = dss_define_structure(handle, sym, rowIdx, nRows, nCols, colPtr, nnz);
        if (error != MKL_DSS_SUCCESS) return error;

        // Reorder the matrix
        error = dss_reorder(handle, opt, 0);
        if (error != MKL_DSS_SUCCESS) return error;

        // Factor the matrix
        dss_int type = (matrixType == 0)
            ? MKL_DSS_POSITIVE_DEFINITE
            : (matrixType == 1)
                ? MKL_DSS_INDEFINITE
                : (matrixType == 2)
                    ? MKL_DSS_HERMITIAN_POSITIVE_DEFINITE
                    : MKL_DSS_HERMITIAN_INDEFINITE;
        error = dss_factor_complex(handle, type, values);
        if (error != MKL_DSS_SUCCESS) return error;

        // Get the solution vector
        error = dss_solve_real(handle, opt, rhsValues, nRhs, solValues);
        if (error != MKL_DSS_SUCCESS) return error;

        // Deallocate solver storage
        error = dss_delete(handle, opt);
        return error;
    }

    DLLEXPORT dss_int z_dss_solve(const dss_int matrixStructure, const dss_int matrixType, const dss_int systemType,
        const dss_int nRows, const dss_int nCols, const dss_int nnz, const dss_int rowIdx[], const dss_int colPtr[], const dss_complex_double values[],
        const dss_int nRhs, const dss_complex_double rhsValues[], dss_complex_double solValues[])
    {
        _MKL_DSS_HANDLE_t handle;
        dss_int error;

        dss_int opt = MKL_DSS_MSG_LVL_WARNING + MKL_DSS_TERM_LVL_ERROR + MKL_DSS_ZERO_BASED_INDEXING + MKL_DSS_AUTO_ORDER;
        if (systemType == 1) opt += MKL_DSS_CONJUGATE_SOLVE; // solve a conjugate transposed system, A¢Óx = b
        else if (systemType == 2) opt += MKL_DSS_TRANSPOSE_SOLVE; // solve a transposed system, A'x = b

        // Initialize the solver
        error = dss_create(handle, opt);
        if (error != MKL_DSS_SUCCESS) return error;

        // Define the non-zero structure of the matrix
        dss_int sym = (matrixStructure == 0)
            ? MKL_DSS_SYMMETRIC_STRUCTURE_COMPLEX
            : (matrixStructure == 1)
                ? MKL_DSS_SYMMETRIC_COMPLEX
                : MKL_DSS_NON_SYMMETRIC_COMPLEX;
        error = dss_define_structure(handle, sym, rowIdx, nRows, nCols, colPtr, nnz);
        if (error != MKL_DSS_SUCCESS) return error;

        // Reorder the matrix
        error = dss_reorder(handle, opt, 0);
        if (error != MKL_DSS_SUCCESS) return error;

        // Factor the matrix
        dss_int type = (matrixType == 0)
            ? MKL_DSS_POSITIVE_DEFINITE
            : (matrixType == 1)
                ? MKL_DSS_INDEFINITE
                : (matrixType == 2)
                    ? MKL_DSS_HERMITIAN_POSITIVE_DEFINITE
                    : MKL_DSS_HERMITIAN_INDEFINITE;
        error = dss_factor_complex(handle, type, values);
        if (error != MKL_DSS_SUCCESS) return error;

        // Get the solution vector
        error = dss_solve_real(handle, opt, rhsValues, nRhs, solValues);
        if (error != MKL_DSS_SUCCESS) return error;

        // Deallocate solver storage
        error = dss_delete(handle, opt);
        return error;
    }  

#if __cplusplus
}
#endif
