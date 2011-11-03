// TracingProfiler.h : Declaration of the CTracingProfiler

#pragma once
#include "resource.h"       // main symbols

#include "CorProfilerCallbackBase.h"

#include "VisualProfilerBackend_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;


// CTracingProfiler

class ATL_NO_VTABLE CTracingProfiler :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTracingProfiler, &CLSID_TracingProfiler>,
	public ITracingProfiler,
	public CorProfilerCallbackBase
{
public:
	CTracingProfiler()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_TRACINGPROFILER)

DECLARE_NOT_AGGREGATABLE(CTracingProfiler)

BEGIN_COM_MAP(CTracingProfiler)
	COM_INTERFACE_ENTRY(ITracingProfiler)
	COM_INTERFACE_ENTRY(ICorProfilerCallback)
	COM_INTERFACE_ENTRY(ICorProfilerCallback2)
	COM_INTERFACE_ENTRY(ICorProfilerCallback3)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:

	virtual HRESULT STDMETHODCALLTYPE Initialize( /* [in] */ IUnknown *pICorProfilerInfoUnk) ;
	static UINT_PTR _stdcall FunctionMapper(FunctionID functionId, BOOL *pbHookFunction);

};

OBJECT_ENTRY_AUTO(__uuidof(TracingProfiler), CTracingProfiler)
