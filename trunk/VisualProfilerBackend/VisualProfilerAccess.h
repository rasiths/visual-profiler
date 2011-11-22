#pragma once
#include "SerializationBuffer.h"
#include "MessageTypes.h"
#include "stdafx.h"
#include <string>
#include "Utils.h"

#define BUFFER_SIZE 256
using namespace std;

class VisualProfilerAccess{

private:
	HANDLE _pipeHandle;
	
public:

	VisualProfilerAccess(){
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

	}

	void StartListening(){
		
	}

};