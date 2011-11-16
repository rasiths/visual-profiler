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
#include "StackWalker.h"
#include "VisualProfilerBackend_i.h"
#include "StackWalker.h"


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
		cout << endl <<"----- Profiler output -----" << endl;
		map<ThreadID, shared_ptr<StatisticalCallTree>> * pStatCallTreeMap = StatisticalCallTree::GetCallTreeMap();
		wstringstream wsout;	
		for(map<ThreadID, shared_ptr<StatisticalCallTree>>::iterator it = pStatCallTreeMap->begin(); it != pStatCallTreeMap->end(); it++ ){
			StatisticalCallTree * pStatCallTree = it->second.get();
			pStatCallTree->ToString(wsout);
			wsout << endl<< endl;
		}
		wcout << wsout.rdbuf();

		int a;
		cin >> a;
	}

public:
	virtual HRESULT STDMETHODCALLTYPE Initialize(IUnknown *pICorProfilerInfoUnk) ;
	virtual HRESULT STDMETHODCALLTYPE ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId) ;
	virtual HRESULT STDMETHODCALLTYPE ThreadCreated(ThreadID threadId) ;
	virtual HRESULT STDMETHODCALLTYPE ThreadDestroyed(ThreadID threadId) ;
	virtual HRESULT STDMETHODCALLTYPE Shutdown();

private:
	shared_ptr<StackWalker> _stackWalker;
};

OBJECT_ENTRY_AUTO(__uuidof(SamplingProfiler), CSamplingProfiler)
