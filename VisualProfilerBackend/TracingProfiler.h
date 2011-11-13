// TracingProfiler.h : Declaration of the CTracingProfiler

#pragma once
#include "resource.h"       // main symbols
#include <iostream>
#include "CorProfilerCallbackBase.h"

#include "VisualProfilerBackend_i.h"
#include "MethodMetadata.h"
#include "ClassMetadata.h"
#include "ModuleMetadata.h"
#include "AssemblyMetadata.h"
#include "ThreadCallTree.h"
#include <set>
#include <map>
#include <stack>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;
using namespace std;


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
		cout << "------------------------------" << endl;
		return S_OK;
	}

	void FinalRelease()
	{
		map<ThreadID, shared_ptr<ThreadCallTree>> * pThreadCallTreeMap = ThreadCallTree::GetThreadCallTreeMap();
		for(map<ThreadID, shared_ptr<ThreadCallTree>>::iterator it = pThreadCallTreeMap->begin(); it != pThreadCallTreeMap->end(); it++ ){
			ThreadCallTree * pThreadCallTree = it->second.get();
			wcout << pThreadCallTree->ToString()<< endl<< endl;
		}
		int a;
		cin >> a;
	}

	
public:

    virtual HRESULT STDMETHODCALLTYPE Initialize(IUnknown *pICorProfilerInfoUnk) ;
	static UINT_PTR STDMETHODCALLTYPE FunctionMapper(FunctionID functionId,void * clientData,  BOOL *pbHookFunction);
//	virtual HRESULT STDMETHODCALLTYPE ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId) ;
	virtual HRESULT STDMETHODCALLTYPE ThreadCreated(ThreadID threadId) ;
	virtual HRESULT STDMETHODCALLTYPE ThreadDestroyed(ThreadID threadId) ;
    virtual HRESULT STDMETHODCALLTYPE RuntimeThreadSuspended(ThreadID threadId) ;
	virtual HRESULT STDMETHODCALLTYPE RuntimeThreadResumed(ThreadID threadId) ;


private:
	static void __stdcall FunctionEnterHook(FunctionIDOrClientID functionIDOrClientID);
	static void __stdcall FunctionLeaveHook(FunctionIDOrClientID functionIDOrClientID);
	
};

OBJECT_ENTRY_AUTO(__uuidof(TracingProfiler), CTracingProfiler)


//
//	void CacheStats(){
//		int methodsCache =		MethodMetadata::CacheSize();
//		int classesCache =		ClassMetadata::CacheSize();
//		int modulesCache =		ModuleMetadata::CacheSize();	
//		int assembliesCache =	AssemblyMetadata::CacheSize();
//		cout<< "methodsCache = " << methodsCache << endl;
//		cout<< "classesCache = " << classesCache << endl;
//		cout<< "modulesCache = " << modulesCache << endl;
//		cout<< "assembliesCache = " << assembliesCache << endl << endl;
//
//		cout<< "methodsCache2 = " <<		MethodMetadata::Count<< endl;
//		cout<< "classesCache2 = " <<		ClassMetadata::Count<< endl;
//		cout<< "modulesCache2 = " <<		ModuleMetadata::Count<< endl;
//		cout<< "assembliesCache2 = " <<	AssemblyMetadata::Count << endl;
//
/*	for (map<ThreadID, set<DWORD>>::iterator it=ThreadIdsMap.begin() ; it != ThreadIdsMap.end(); it++ ){
			cout << "managed threadId = " << (*it).first << endl;
			set<DWORD> * setOsIds = &(*it).second;
			for (set<DWORD>::iterator it = setOsIds->begin() ; it != setOsIds->end(); it++ ){
				cout << "\tos threadId = " << *it << endl;
			}
			cout << endl;
		}*/
//}