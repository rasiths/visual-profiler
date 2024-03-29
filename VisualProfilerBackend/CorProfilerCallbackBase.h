#pragma once
#include "stdafx.h"
#include <atlcomcli.h>
#include <cor.h>
#include <corprof.h>
#include "VisualProfilerAccess.h"

using namespace ATL;


class CorProfilerCallbackBase :
	public ICorProfilerCallback3 
{
//protected:
public:
	CComQIPtr<ICorProfilerInfo3> pProfilerInfo;
public:

	//------------------------------------------------------------------ICorProfilerCallback3------------------------------------------------------------------
	virtual HRESULT STDMETHODCALLTYPE InitializeForAttach( 
		/* [in] */ IUnknown *pCorProfilerInfoUnk,
		/* [in] */ void *pvClientData,
		/* [in] */ UINT cbClientData) ;

	virtual HRESULT STDMETHODCALLTYPE ProfilerAttachComplete( void) ;

	virtual HRESULT STDMETHODCALLTYPE ProfilerDetachSucceeded( void) ;

	//------------------------------------------------------------------ICorProfilerCallback2------------------------------------------------------------------
	virtual HRESULT STDMETHODCALLTYPE ThreadNameChanged( 
		/* [in] */ ThreadID threadId,
		/* [in] */ ULONG cchName,
		/* [in] */ 
		__in_ecount_opt(cchName)  WCHAR name[  ]) ;

	virtual HRESULT STDMETHODCALLTYPE GarbageCollectionStarted( 
		/* [in] */ int cGenerations,
		/* [size_is][in] */ BOOL generationCollected[  ],
		/* [in] */ COR_PRF_GC_REASON reason) ;

	virtual HRESULT STDMETHODCALLTYPE SurvivingReferences( 
		/* [in] */ ULONG cSurvivingObjectIDRanges,
		/* [size_is][in] */ ObjectID objectIDRangeStart[  ],
		/* [size_is][in] */ ULONG cObjectIDRangeLength[  ]) ;

	virtual HRESULT STDMETHODCALLTYPE GarbageCollectionFinished( void) ;

	virtual HRESULT STDMETHODCALLTYPE FinalizeableObjectQueued( 
		/* [in] */ DWORD finalizerFlags,
		/* [in] */ ObjectID objectID) ;

	virtual HRESULT STDMETHODCALLTYPE RootReferences2( 
		/* [in] */ ULONG cRootRefs,
		/* [size_is][in] */ ObjectID rootRefIds[  ],
		/* [size_is][in] */ COR_PRF_GC_ROOT_KIND rootKinds[  ],
		/* [size_is][in] */ COR_PRF_GC_ROOT_FLAGS rootFlags[  ],
		/* [size_is][in] */ UINT_PTR rootIds[  ]) ;

	virtual HRESULT STDMETHODCALLTYPE HandleCreated( 
		/* [in] */ GCHandleID handleId,
		/* [in] */ ObjectID initialObjectId) ;

	virtual HRESULT STDMETHODCALLTYPE HandleDestroyed( 
		/* [in] */ GCHandleID handleId) ;

	//------------------------------------------------------------------ICorProfilerCallback------------------------------------------------------------------
	virtual HRESULT STDMETHODCALLTYPE Initialize( 
		/* [in] */ IUnknown *pICorProfilerInfoUnk) ;

	virtual HRESULT STDMETHODCALLTYPE Shutdown( void) ;

	virtual HRESULT STDMETHODCALLTYPE AppDomainCreationStarted( 
		/* [in] */ AppDomainID appDomainId) ;

	virtual HRESULT STDMETHODCALLTYPE AppDomainCreationFinished( 
		/* [in] */ AppDomainID appDomainId,
		/* [in] */ HRESULT hrStatus) ;

	virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownStarted( 
		/* [in] */ AppDomainID appDomainId) ;

	virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownFinished( 
		/* [in] */ AppDomainID appDomainId,
		/* [in] */ HRESULT hrStatus) ;

	virtual HRESULT STDMETHODCALLTYPE AssemblyLoadStarted( 
		/* [in] */ AssemblyID assemblyId) ;

	virtual HRESULT STDMETHODCALLTYPE AssemblyLoadFinished( 
		/* [in] */ AssemblyID assemblyId,
		/* [in] */ HRESULT hrStatus) ;

	virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadStarted( 
		/* [in] */ AssemblyID assemblyId) ;

	virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadFinished( 
		/* [in] */ AssemblyID assemblyId,
		/* [in] */ HRESULT hrStatus) ;

	virtual HRESULT STDMETHODCALLTYPE ModuleLoadStarted( 
		/* [in] */ ModuleID moduleId) ;

	virtual HRESULT STDMETHODCALLTYPE ModuleLoadFinished( 
		/* [in] */ ModuleID moduleId,
		/* [in] */ HRESULT hrStatus) ;

	virtual HRESULT STDMETHODCALLTYPE ModuleUnloadStarted( 
		/* [in] */ ModuleID moduleId) ;

	virtual HRESULT STDMETHODCALLTYPE ModuleUnloadFinished( 
		/* [in] */ ModuleID moduleId,
		/* [in] */ HRESULT hrStatus) ;

	virtual HRESULT STDMETHODCALLTYPE ModuleAttachedToAssembly( 
		/* [in] */ ModuleID moduleId,
		/* [in] */ AssemblyID AssemblyId) ;

	virtual HRESULT STDMETHODCALLTYPE ClassLoadStarted( 
		/* [in] */ ClassID classId) ;

	virtual HRESULT STDMETHODCALLTYPE ClassLoadFinished( 
		/* [in] */ ClassID classId,
		/* [in] */ HRESULT hrStatus) ;

	virtual HRESULT STDMETHODCALLTYPE ClassUnloadStarted( 
		/* [in] */ ClassID classId) ;

	virtual HRESULT STDMETHODCALLTYPE ClassUnloadFinished( 
		/* [in] */ ClassID classId,
		/* [in] */ HRESULT hrStatus) ;

	virtual HRESULT STDMETHODCALLTYPE FunctionUnloadStarted( 
		/* [in] */ FunctionID functionId) ;

	virtual HRESULT STDMETHODCALLTYPE JITCompilationStarted( 
		/* [in] */ FunctionID functionId,
		/* [in] */ BOOL fIsSafeToBlock) ;

	virtual HRESULT STDMETHODCALLTYPE JITCompilationFinished( 
		/* [in] */ FunctionID functionId,
		/* [in] */ HRESULT hrStatus,
		/* [in] */ BOOL fIsSafeToBlock) ;

	virtual HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchStarted( 
		/* [in] */ FunctionID functionId,
		/* [out] */ BOOL *pbUseCachedFunction) ;

	virtual HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchFinished( 
		/* [in] */ FunctionID functionId,
		/* [in] */ COR_PRF_JIT_CACHE result) ;

	virtual HRESULT STDMETHODCALLTYPE JITFunctionPitched( 
		/* [in] */ FunctionID functionId) ;

	virtual HRESULT STDMETHODCALLTYPE JITInlining( 
		/* [in] */ FunctionID callerId,
		/* [in] */ FunctionID calleeId,
		/* [out] */ BOOL *pfShouldInline) ;

	virtual HRESULT STDMETHODCALLTYPE ThreadCreated( 
		/* [in] */ ThreadID threadId) ;

	virtual HRESULT STDMETHODCALLTYPE ThreadDestroyed( 
		/* [in] */ ThreadID threadId) ;

	virtual HRESULT STDMETHODCALLTYPE ThreadAssignedToOSThread( 
		/* [in] */ ThreadID managedThreadId,
		/* [in] */ DWORD osThreadId) ;

	virtual HRESULT STDMETHODCALLTYPE RemotingClientInvocationStarted( void) ;

	virtual HRESULT STDMETHODCALLTYPE RemotingClientSendingMessage( 
		/* [in] */ GUID *pCookie,
		/* [in] */ BOOL fIsAsync) ;

	virtual HRESULT STDMETHODCALLTYPE RemotingClientReceivingReply( 
		/* [in] */ GUID *pCookie,
		/* [in] */ BOOL fIsAsync) ;

	virtual HRESULT STDMETHODCALLTYPE RemotingClientInvocationFinished( void) ;

	virtual HRESULT STDMETHODCALLTYPE RemotingServerReceivingMessage( 
		/* [in] */ GUID *pCookie,
		/* [in] */ BOOL fIsAsync) ;

	virtual HRESULT STDMETHODCALLTYPE RemotingServerInvocationStarted( void) ;

	virtual HRESULT STDMETHODCALLTYPE RemotingServerInvocationReturned( void) ;

	virtual HRESULT STDMETHODCALLTYPE RemotingServerSendingReply( 
		/* [in] */ GUID *pCookie,
		/* [in] */ BOOL fIsAsync) ;

	virtual HRESULT STDMETHODCALLTYPE UnmanagedToManagedTransition( 
		/* [in] */ FunctionID functionId,
		/* [in] */ COR_PRF_TRANSITION_REASON reason) ;

	virtual HRESULT STDMETHODCALLTYPE ManagedToUnmanagedTransition( 
		/* [in] */ FunctionID functionId,
		/* [in] */ COR_PRF_TRANSITION_REASON reason) ;

	virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendStarted( 
		/* [in] */ COR_PRF_SUSPEND_REASON suspendReason) ;

	virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendFinished( void) ;

	virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendAborted( void) ;

	virtual HRESULT STDMETHODCALLTYPE RuntimeResumeStarted( void) ;

	virtual HRESULT STDMETHODCALLTYPE RuntimeResumeFinished( void) ;

	virtual HRESULT STDMETHODCALLTYPE RuntimeThreadSuspended( 
		/* [in] */ ThreadID threadId) ;

	virtual HRESULT STDMETHODCALLTYPE RuntimeThreadResumed( 
		/* [in] */ ThreadID threadId) ;

	virtual HRESULT STDMETHODCALLTYPE MovedReferences( 
		/* [in] */ ULONG cMovedObjectIDRanges,
		/* [size_is][in] */ ObjectID oldObjectIDRangeStart[  ],
		/* [size_is][in] */ ObjectID newObjectIDRangeStart[  ],
		/* [size_is][in] */ ULONG cObjectIDRangeLength[  ]) ;

	virtual HRESULT STDMETHODCALLTYPE ObjectAllocated( 
		/* [in] */ ObjectID objectId,
		/* [in] */ ClassID classId) ;

	virtual HRESULT STDMETHODCALLTYPE ObjectsAllocatedByClass( 
		/* [in] */ ULONG cClassCount,
		/* [size_is][in] */ ClassID classIds[  ],
		/* [size_is][in] */ ULONG cObjects[  ]) ;

	virtual HRESULT STDMETHODCALLTYPE ObjectReferences( 
		/* [in] */ ObjectID objectId,
		/* [in] */ ClassID classId,
		/* [in] */ ULONG cObjectRefs,
		/* [size_is][in] */ ObjectID objectRefIds[  ]) ;

	virtual HRESULT STDMETHODCALLTYPE RootReferences( 
		/* [in] */ ULONG cRootRefs,
		/* [size_is][in] */ ObjectID rootRefIds[  ]) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionThrown( 
		/* [in] */ ObjectID thrownObjectId) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionEnter( 
		/* [in] */ FunctionID functionId) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionLeave( void) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFilterEnter( 
		/* [in] */ FunctionID functionId) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFilterLeave( void) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionSearchCatcherFound( 
		/* [in] */ FunctionID functionId) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionOSHandlerEnter( 
		/* [in] */ UINT_PTR __unused) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionOSHandlerLeave( 
		/* [in] */ UINT_PTR __unused) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionEnter( 
		/* [in] */ FunctionID functionId) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionLeave( void) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyEnter( 
		/* [in] */ FunctionID functionId) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyLeave( void) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionCatcherEnter( 
		/* [in] */ FunctionID functionId,
		/* [in] */ ObjectID objectId) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionCatcherLeave( void) ;

	virtual HRESULT STDMETHODCALLTYPE COMClassicVTableCreated( 
		/* [in] */ ClassID wrappedClassId,
		/* [in] */ REFGUID implementedIID,
		/* [in] */ void *pVTable,
		/* [in] */ ULONG cSlots) ;

	virtual HRESULT STDMETHODCALLTYPE COMClassicVTableDestroyed( 
		/* [in] */ ClassID wrappedClassId,
		/* [in] */ REFGUID implementedIID,
		/* [in] */ void *pVTable) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherFound( void) ;

	virtual HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherExecute( void) ;

};

