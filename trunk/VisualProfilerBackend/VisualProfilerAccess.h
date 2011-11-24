#pragma once
#include "SerializationBuffer.h"
#include "MetadataTypes.h"
#include "stdafx.h"
#include <string>
#include "Utils.h"
#include <map>
#include "ThreadCallTree.h"

#define BUFFER_SIZE 256
using namespace std;

typedef UINT Commands;
#define Commands_SendProfilingData 101

class VisualProfilerAccess{

private:
	HANDLE _pipeHandle;
	HANDLE _listeningThread;
	bool _stopListening;

public:

	VisualProfilerAccess(){
		//return;
		_pipeHandle = INVALID_HANDLE_VALUE;

		wstring pipeName = GetEnvirnomentalVariable(L"VisualProfiler.PipeName");
		bool pipeNameNotFound = pipeName.size() == 0;
		if(pipeNameNotFound)
			return;

		wstring pipeNameExtended(L"\\\\.\\pipe\\");
		pipeNameExtended.append(pipeName);

		_pipeHandle = CreateFile(pipeNameExtended.data(), GENERIC_READ| GENERIC_WRITE, 0, 0, OPEN_EXISTING, 0, 0);
		CheckError(_pipeHandle != INVALID_HANDLE_VALUE);
	}

	~VisualProfilerAccess(){
		CloseHandle(_pipeHandle);
	}
	
	static DWORD WINAPI StartListening(void * data){
		VisualProfilerAccess * _pThis = (VisualProfilerAccess *) data;
		_pThis->_stopListening = false;
		while(!_pThis->_stopListening){
			BYTE buffer[sizeof(Commands)];
			DWORD bytesReadCount = -1;
			BOOL succeeded = ReadFile(_pThis->_pipeHandle, &buffer, sizeof(Commands), &bytesReadCount, NULL);
			
			if(succeeded){
				SerializationBuffer bufferProfilingData;
				map<ThreadID, shared_ptr<ThreadCallTree>> * callTreeMap = ThreadCallTree::GetCallTreeMap();
				map<ThreadID, shared_ptr<ThreadCallTree>>::iterator it = callTreeMap->begin();
				for(; it != callTreeMap->end(); it++){
					it->second->CopyCallTreeBufferToBuffer(&bufferProfilingData);
				}

				SerializationBuffer bufferMetadata;
				AssemblyMetadata::SerializeMetadata(&bufferMetadata);
				ModuleMetadata::SerializeMetadata(&bufferMetadata);
				ClassMetadata::SerializeMetadata(&bufferMetadata);
				MethodMetadata::SerializeMetadata(&bufferMetadata);

				SerializationBuffer bufferAll;
				bufferAll.SerializeUINT( SIZE_OF_UINT + bufferMetadata.Size() + bufferProfilingData.Size());
				bufferAll.SerializeUINT(bufferMetadata.Size());
				bufferMetadata.CopyToAnotherBuffer(&bufferAll);
				bufferProfilingData.CopyToAnotherBuffer(&bufferAll);
				DWORD written = 0;
				WriteFile(_pThis->_pipeHandle,bufferAll.GetBuffer(),bufferAll.Size(),&written,0);
			}	
		}

		DWORD success = 1;
		return success;
	}

	void StartListeningAsync(){
		_listeningThread = CreateThread(NULL,NULL,StartListening, this, NULL, NULL);
	}

	void StopListening(){
		_stopListening = true;
		CancelIoEx(_pipeHandle, NULL);
		wstring message = L"Bye Bye";
		DWORD written = 0;
		//WriteFile(_pipeHandle, message.data(), message.size(), &written, NULL);
	}
};