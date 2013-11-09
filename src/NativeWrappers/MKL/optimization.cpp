#include <algorithm>
#include <complex>
#define MKL_Complex8 std::complex<float>
#define MKL_Complex16 std::complex<double>

#include "mkl_lapack.h"
#include "mkl_cblas.h"
#include "lapack_common.h"
#include "wrapper_common.h"
#include "mkl_lapacke.h"
#include "mkl.h"
#include "mkl_trans.h"

template<typename T>
inline MKL_INT lu_factor_2(MKL_INT m, T a[], MKL_INT ipiv[],
                         void (*getrf)(const MKL_INT*, const MKL_INT*, T*, const MKL_INT*, MKL_INT*, MKL_INT*))
{
    std::complex<double> x = 5;
	MKL_INT info = 0;
    getrf(&m, &m, a, &m, ipiv, &info);
    shift_ipiv_down(m, ipiv);
    return info;
};

extern "C" {

//_Mkl_Api(MKL_INT,dtrnlsp_init,(_TRNSP_HANDLE_t*, MKL_INT*, MKL_INT*, double*, double*, MKL_INT*, MKL_INT*, double*))
//_Mkl_Api(MKL_INT,dtrnlsp_check,(_TRNSP_HANDLE_t*, MKL_INT*, MKL_INT*, double*, double*, double*, MKL_INT*))
//_Mkl_Api(MKL_INT,dtrnlsp_solve,(_TRNSP_HANDLE_t*, double*, double*, MKL_INT*))
//_Mkl_Api(MKL_INT,dtrnlsp_get,(_TRNSP_HANDLE_t*, MKL_INT*, MKL_INT*, double*, double*))
//_Mkl_Api(MKL_INT,dtrnlsp_delete,(_TRNSP_HANDLE_t*))
//
//_Mkl_Api(MKL_INT,dtrnlspbc_init,(_TRNSPBC_HANDLE_t*, MKL_INT*, MKL_INT*, double*, double*, double*, double*, MKL_INT*, MKL_INT*, double*))
//_Mkl_Api(MKL_INT,dtrnlspbc_check,(_TRNSPBC_HANDLE_t*, MKL_INT*, MKL_INT*, double*, double*, double*, double*, double*, MKL_INT*))
//_Mkl_Api(MKL_INT,dtrnlspbc_solve,(_TRNSPBC_HANDLE_t*, double*, double*, MKL_INT*))
//_Mkl_Api(MKL_INT,dtrnlspbc_get,(_TRNSPBC_HANDLE_t*, MKL_INT*, MKL_INT*, double*, double*))
//_Mkl_Api(MKL_INT,dtrnlspbc_delete,(_TRNSPBC_HANDLE_t*))
//
//_Mkl_Api(MKL_INT,djacobi_init,(_JACOBIMATRIX_HANDLE_t*, MKL_INT*, MKL_INT*, double*, double*, double*))
//_Mkl_Api(MKL_INT,djacobi_solve,(_JACOBIMATRIX_HANDLE_t*, double*, double*, MKL_INT*))
//_Mkl_Api(MKL_INT,djacobi_delete,(_JACOBIMATRIX_HANDLE_t*))
//_Mkl_Api(MKL_INT,djacobi,(USRFCND fcn, MKL_INT*, MKL_INT*, double*, double*, double*))
//_Mkl_Api(MKL_INT,djacobix,(USRFCNXD fcn, MKL_INT*, MKL_INT*, double*, double*, double*,void*))

	typedef double (__stdcall *function)();

	DLLEXPORT double Test(function func, double input[])
	{
		input[0] = 5;
		return func();
	}

    DLLEXPORT MKL_INT unbound_nonlinearleastsq_init(_TRNSPBC_HANDLE_t* handle, MKL_INT n, MKL_INT m, double x[], double eps[], MKL_INT iter1, MKL_INT iter2, double rs)
    {
		return dtrnlsp_init(handle, &n, &m, x, eps, &iter1, &iter2, &rs);
	}

    DLLEXPORT MKL_INT unbound_nonlinearleastsq_check(_TRNSPBC_HANDLE_t* handle, MKL_INT n, MKL_INT m, double fjac[], double fvec[], double eps[], int info[])
    {
		return dtrnlsp_check(handle, &n, &m, fjac, fvec, eps, info);
	}

	DLLEXPORT MKL_INT unbound_nonlinearleastsq_solve(_TRNSPBC_HANDLE_t* handle, double fvec[], double fjac[], int* RCI_Request)
    {
		return dtrnlsp_solve(handle, fvec, fjac, RCI_Request);
    }

	DLLEXPORT MKL_INT unbound_nonlinearleastsq_get(_TRNSPBC_HANDLE_t* handle, MKL_INT* iter, MKL_INT* st_cr, double* r1, double* r2)
    {
		return dtrnlsp_get(handle, iter, st_cr, r1, r2);
    }

	DLLEXPORT MKL_INT unbound_nonlinearleastsq_delete(_TRNSPBC_HANDLE_t* handle)
    {
		return 	dtrnlsp_delete(handle);
    }

	DLLEXPORT MKL_INT jacobi_init(_JACOBIMATRIX_HANDLE_t* handle, MKL_INT n, MKL_INT m, double x[], double fjac[], double eps)
	{
		return djacobi_init(handle, &n, &m, x, fjac, &eps);
	}

	DLLEXPORT MKL_INT jacobi_solve(_JACOBIMATRIX_HANDLE_t* handle, double f1[], double f2[], MKL_INT* RCI_Request)
	{
		return djacobi_solve(handle, f1, f2, RCI_Request);
	}

	DLLEXPORT MKL_INT jacobi_delete(_TRNSPBC_HANDLE_t* handle)
    {
		return djacobi_delete(handle);
    }

	DLLEXPORT void FreeBuffers()
	{
		MKL_FreeBuffers();
	}

	DLLEXPORT MKL_INT unbound_nonlinearleastsq(double parameters[], double parametersInitialGuess[], MKL_INT parametersLength, 
		double residuals[], MKL_INT residualsLength, 
		double jacobian[], // size parametersLength * residualsLength
		double residualsMinus[], function updateResidualsMinus,
		double residualsPlus[], function updateResidualsPlus,
		function updateResiduals)
	{
		double eps[6]; // stop criteria
		MKL_INT i;
		for (i = 0; i < 6; i++)
			eps[i] = 0.00001;

		MKL_INT RCI_Request;     
		MKL_INT successful;

		MKL_INT maxIterations = 1000, maxTrialStepIterations = 100;

		double rs = 0.0;
		_TRNSP_HANDLE_t solverHandle;  
		_JACOBIMATRIX_HANDLE_t jacobianHandle;

		MKL_INT info[6]; // for parameter checking

		double initialStepBound = 0.0;

		double jacobianPrecision = 0.000001;

		MKL_INT iteration;

		double initialResidual, finalResidual;

		// zero initial values:
		for (i = 0; i < residualsLength; i++)
			residuals[i] = 0.0;
		for (i = 0; i < residualsLength * parametersLength; i++)
			jacobian[i] = 0.0;

		if (dtrnlsp_init(&solverHandle, &parametersLength, &residualsLength, parameters, eps, &maxIterations, &maxTrialStepIterations, &initialStepBound) !=
			TR_SUCCESS)
		{                             
			MKL_FreeBuffers();
			return 1;
		}
	 
		if (dtrnlsp_check(&solverHandle, &parametersLength, &residualsLength, jacobian, residuals, eps, info) != TR_SUCCESS)
		{                               
			MKL_FreeBuffers();
			return 1;
		}
		else
		{
			if (info[0] != 0 || // Handle invalid
				info[1] != 0 || // Jacobian array not valid
				info[2] != 0 || // Parameters array not valid
				info[3] != 0)   // Eps array not valid
			{                                  
				MKL_FreeBuffers();
				return 1;
			}
		}

		if (djacobi_init(&jacobianHandle, &parametersLength, &residualsLength, parameters, jacobian, &jacobianPrecision) != TR_SUCCESS)
		{
			MKL_FreeBuffers ();
			return 1;
		}

		RCI_Request = 0;
		successful = 0;
		while (successful == 0)
		{
			if (dtrnlsp_solve(&solverHandle, residuals, jacobian, &RCI_Request) != TR_SUCCESS)
			{                             
				MKL_FreeBuffers();
				return 1;
			}
			if (RCI_Request == -1 || RCI_Request == -2 || RCI_Request == -3 ||
				RCI_Request == -4 || RCI_Request == -5 || RCI_Request == -6)
				successful = 1;
			if (RCI_Request == 1) // recalculate function to update parameters
			{
				updateResiduals();
			}
			if (RCI_Request == 2)
			{
				MKL_INT rci_request = 0;
				MKL_INT jacobianSuccessful = 0;
				
				// update Jacobian matrix:
				while (jacobianSuccessful == 0)
				{
					if (djacobi_solve (&jacobianHandle, residualsPlus, residualsMinus, &rci_request) != TR_SUCCESS)
					{
						MKL_FreeBuffers ();
						return 1;
					}
					if (rci_request == 1)
						updateResidualsPlus();
					else if (rci_request == 2)
						updateResidualsMinus();
					else if (rci_request == 0)
						jacobianSuccessful = 1;
				}
			}
		}

		MKL_INT stopCriterionNumber, iterations;
		if (dtrnlsp_get (&solverHandle, &iterations, &stopCriterionNumber, &initialResidual, &finalResidual) != TR_SUCCESS)
		{                                  
			MKL_FreeBuffers();
			return 1;
		}

		if (dtrnlsp_delete (&solverHandle) != TR_SUCCESS)
		{                                  
			MKL_FreeBuffers();
			return 1;
		}

		if (djacobi_delete (&jacobianHandle) != TR_SUCCESS)
		{
			MKL_FreeBuffers ();
			return 1;
		}
                          
		MKL_FreeBuffers ();
		return 0;
	}
  
}