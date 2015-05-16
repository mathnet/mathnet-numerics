template <typename _T>
struct complex
{
	_T real, imag;

	complex(_T _real = 0, _T _imag = 0)
	{
		real = _real;
		imag = _imag;
	}

	complex(const complex<_T>& right)
	{
		real = right.real;
		imag = right.imag;
	}

	complex& operator=(const complex& right)
	{
		real = right.real;
		imag = right.imag;
		return *this;
	}

	complex& operator=(const _T& right)
	{
		real = right;
		imag = 0;
		return *this;
	}

	template<typename _Other> inline
		complex& operator=(const complex<_Other>& right)
	{
		real = (_T)right.real;
		imag = (_T)right.imag;
		return *this;
	}
};