// SamplingProfiler.h : Declaration of the CSamplingProfiler

#pragma once
#include "resource.h"       // main symbols
#include "CorProfilerCallbackBase.h"
#include <iostream>
#include <vector>
#include <windows.h>
#include <process.h> 

#include <set>
#include "CriticalSection.h"
#include "MethodMetadata.h"

#include "VisualProfilerBackend_i.h"



#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;


// CSamplingProfiler

class ATL_NO_VTABLE CSamplingProfiler :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSamplingProfiler, &CLSID_SamplingProfiler>,
	public ISamplingProfiler,
	public CorProfilerCallbackBase
{
public:
	CSamplingProfiler()
	{
	}

	DECLARE_REGISTRY_RESOURCEID(IDR_SAMPLINGPROFILER)

	DECLARE_NOT_AGGREGATABLE(CSamplingProfiler)

	BEGIN_COM_MAP(CSamplingProfiler)
		COM_INTERFACE_ENTRY(ISamplingProfiler)
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
		_continueSampling = false;
		for(vector<FunctionID>::iterator it2 = _functionIds.begin(); it2 != _functionIds.end(); it2++){
					FunctionID functionId = *it2;
					if(functionId == 0)
						continue;
					shared_ptr<MethodMetadata> pMethodMetadata = MethodMetadata::GetById(functionId);
					wcout << "\t" << pMethodMetadata->ToString() << endl;
				}
		int a;
		cin >> a;
	}



public:

	virtual HRESULT STDMETHODCALLTYPE Initialize( /* [in] */ IUnknown *pICorProfilerInfoUnk) ;
	virtual HRESULT STDMETHODCALLTYPE ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId) ;
	virtual HRESULT STDMETHODCALLTYPE ThreadCreated(ThreadID threadId) ;
	virtual HRESULT STDMETHODCALLTYPE ThreadDestroyed(ThreadID threadId) ;

	static bool _continueSampling;
	static vector<FunctionID> _functionIds;
};

OBJECT_ENTRY_AUTO(__uuidof(SamplingProfiler), CSamplingProfiler)
