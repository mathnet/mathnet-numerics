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

        dss_int opt = MKL_DSS_MSG_LVL_WARNING + MKL_DSS_TERM_LVL_ERROR + MKL_DSS_ZERO_BASED_INDEXING + MKL_DSS_AUTO_ORDER;
        opt += systemType;

        // Initialize the solver
        error = dss_create(handle, opt);
        if (error != MKL_DSS_SUCCESS) return error;

        // Define the non-zero structure of the matrix
        error = dss_define_structure(handle, matrixStructure, rowIdx, nRows, nCols, colPtr, nnz);
        if (error != MKL_DSS_SUCCESS) return error;

        // Reorder the matrix
        error = dss_reorder(handle, opt, 0);
        if (error != MKL_DSS_SUCCESS) return error;

        // Factor the matrix
        error = dss_factor_real(handle, matrixType, values);
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
        opt += systemType;

        // Initialize the solver
        error = dss_create(handle, opt);
        if (error != MKL_DSS_SUCCESS) return error;

        // Define the non-zero structure of the matrix
        error = dss_define_structure(handle, matrixStructure, rowIdx, nRows, nCols, colPtr, nnz);
        if (error != MKL_DSS_SUCCESS) return error;

        // Reorder the matrix
        error = dss_reorder(handle, opt, 0);
        if (error != MKL_DSS_SUCCESS) return error;

        // Factor the matrix
        error = dss_factor_real(handle, matrixType, values);
        if (error != MKL_DSS_SUCCESS) return error;

        // Get the solution vector
        error = dss_solve_real(handle, opt, rhsValues, nRhs, solValues);
        if (error != MKL_DSS_SUCCESS) return error;

        // Deallocate solver storage
        error = dss_delete(handle, opt);
        return error;
    }

    DLLEXPORT dss_int c_dss_solve(const dss_int matrixStructure, const dss_int matrixType, const dss_int systemType,
        const dss_int nRows, const dss_int nCols, const dss_int nnz, const dss_int rowIdx[], const dss_int colPtr[], const dss_complex_float values[],
        const dss_int nRhs, const dss_complex_float rhsValues[], dss_complex_float solValues[])
    {
        _MKL_DSS_HANDLE_t handle;
        dss_int error;

        dss_int opt = MKL_DSS_MSG_LVL_WARNING + MKL_DSS_TERM_LVL_ERROR + MKL_DSS_ZERO_BASED_INDEXING + MKL_DSS_AUTO_ORDER;
        opt += systemType;

        // Initialize the solver
        error = dss_create(handle, opt);
        if (error != MKL_DSS_SUCCESS) return error;

        // Define the non-zero structure of the matrix
        error = dss_define_structure(handle, matrixStructure, rowIdx, nRows, nCols, colPtr, nnz);
        if (error != MKL_DSS_SUCCESS) return error;

        // Reorder the matrix
        error = dss_reorder(handle, opt, 0);
        if (error != MKL_DSS_SUCCESS) return error;

        // Factor the matrix
        error = dss_factor_complex(handle, matrixType, values);
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
        opt += systemType;

        // Initialize the solver
        error = dss_create(handle, opt);
        if (error != MKL_DSS_SUCCESS) return error;

        // Define the non-zero structure of the matrix
        error = dss_define_structure(handle, matrixStructure, rowIdx, nRows, nCols, colPtr, nnz);
        if (error != MKL_DSS_SUCCESS) return error;

        // Reorder the matrix
        error = dss_reorder(handle, opt, 0);
        if (error != MKL_DSS_SUCCESS) return error;

        // Factor the matrix
        error = dss_factor_complex(handle, matrixType, values);
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
