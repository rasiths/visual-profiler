// SamplingProfiler.cpp : Implementation of CSamplingProfiler

#include "stdafx.h"
#include "SamplingProfiler.h"


HRESULT STDMETHODCALLTYPE CSamplingProfiler::Initialize( /* [in] */ IUnknown *pICorProfilerInfoUnk) {
	CorProfilerCallbackBase::Initialize(pICorProfilerInfoUnk);
	Beep(2000, 100);
	return S_OK;
}
