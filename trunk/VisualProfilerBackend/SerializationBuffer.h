#pragma once




#include <cor.h>
#include <corprof.h>
#include <string>
#include "MessageTypes.h"

#define INITIAL_BUFFER_SIZE 1024
#define SIZE_OF_MESSAGETYPES sizeof(MessageTypes)
#define SIZE_OF_UINT_PTR sizeof(UINT_PTR)
#define SIZE_OF_MDTOKEN sizeof(mdToken)
#define SIZE_OF_WCHAR sizeof(WCHAR)
#define SIZE_OF_UINT sizeof(UINT)
#define SIZE_OF_BOOL sizeof(bool)


////Frontend -> Backend
//#define	MessageType_SendMetadata = 1,
//#define	MessageType_SendProfilingData = 2,
//#define	MessageType_Terminate = 3,
//
////Backend -> Frontend
//#define	MessageType_AssemblyMetadata = 11,
//#define	MessageType_ModuleMedatada = 12,
//#define	MessageType_ClassMedatada = 13,
//#define	MessageType_MethodMedatada = 14,
//#define	MessageType_ProfilingData = 15

using namespace std;

class SerializationBuffer
{
public:
	enum MessageTypes{
		//Frontend -> Backend
		SendMetadata = 1,
		SendProfilingData = 2,
		Terminate = 3,

		//Backend -> Frontend
		AssemblyMetadata = 11,
		ModuleMedatada = 12,
		ClassMedatada = 13,
		MethodMedatada = 14,
		ProfilingData = 15
	};


	SerializationBuffer(void):_currentIndex(0),_bufferSize(INITIAL_BUFFER_SIZE){
		_buffer = new BYTE[INITIAL_BUFFER_SIZE];
		ZeroBuffer(_buffer, _bufferSize);
	}

	~SerializationBuffer(void){
		delete [] _buffer;
		_bufferSize = _currentIndex= -1;
		_buffer = NULL;
	}

	void SerializeMetadataId(const UINT_PTR & metadataId){
		CopyToBuffer(&metadataId, SIZE_OF_UINT_PTR);
	}

	void SerializeMdToken(const mdToken & mdToken){
		CopyToBuffer(&mdToken, SIZE_OF_MDTOKEN);
	}

	void SerializeUINT(const UINT & uint){
		CopyToBuffer(&uint, SIZE_OF_UINT);
	}

	void SerializeBool(const bool & boolean){
		CopyToBuffer(&boolean, SIZE_OF_BOOL);
	}

	void SerializeWString(const wstring & str){
		UINT byteCountOfStr =  str.size() * SIZE_OF_WCHAR;
		SerializeUINT(byteCountOfStr);

		const WCHAR * wchars = str.data();
		CopyToBuffer(wchars, byteCountOfStr);
	}

	void SerializeMessageTypes(const MessageTypes & messageType){
		CopyToBuffer(&messageType, SIZE_OF_MESSAGETYPES);
	}

private:
	void EnsureBufferCapacity(UINT requiredCapacity){
		bool capicityExceeded = _bufferSize < (_currentIndex + requiredCapacity );
		if(capicityExceeded){
			UINT newBufferSize = _bufferSize + INITIAL_BUFFER_SIZE;
			BYTE * newBuffer = new BYTE[newBufferSize];
			ZeroBuffer(newBuffer, newBufferSize);
			memcpy_s(newBuffer,newBufferSize, _buffer, _bufferSize);
			delete [] _buffer;
			_buffer = newBuffer;
			_bufferSize = newBufferSize;
		}
	}

	void CopyToBuffer(const void * sourceAddr, UINT numberOfBytes){
		EnsureBufferCapacity(SIZE_OF_UINT_PTR);
		BYTE* destinationAddr = _buffer + _currentIndex;
		memcpy(destinationAddr, sourceAddr, numberOfBytes);
		_currentIndex += numberOfBytes;
	}

	//TODO Remove this method, just debugging
	void ZeroBuffer(BYTE * startAddr, UINT byteCount){
		for(UINT i = 0; i < byteCount; i++){
			startAddr[i] = 0;
		}
	}


private:
	UINT _currentIndex;
	BYTE* _buffer;
	UINT _bufferSize;
};

