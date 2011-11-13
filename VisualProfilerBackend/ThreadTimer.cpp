#include "StdAfx.h"
#include "ThreadTimer.h"

ThreadTimer::ThreadTimer(){
	Reset();
}

void ThreadTimer::Start(){
	_isStopped = false;
	GetSystemTimeAsFileTime(&_startTime);
}

void ThreadTimer::GetElapsedTimeIn100NanoSeconds(ULONGLONG * elapsedTime){
	if(_isStopped){
		*elapsedTime = _elapsedTime;
	}else{
		SubtractCurrentFromStartAndAddElapsedTime(elapsedTime); 
	}
}

void ThreadTimer::Stop(){
	SubtractCurrentFromStartAndAddElapsedTime(&_elapsedTime);
	_isStopped = true;
}

void ThreadTimer::Reset(){
	_elapsedTime = 0;
	_isStopped = true;	
}

void ThreadTimer::SubtractCurrentFromStartAndAddElapsedTime(ULONGLONG * result ){
	FILETIME currentTime;
	GetSystemTimeAsFileTime(&currentTime);
	
	ULARGE_INTEGER temp1;
	temp1.HighPart = currentTime.dwHighDateTime;
	temp1.LowPart =  currentTime.dwLowDateTime;
	
	ULARGE_INTEGER temp2;
	temp2.HighPart = _startTime.dwHighDateTime;
	temp2.LowPart =  _startTime.dwLowDateTime;
	
	*result = temp1.QuadPart - temp2.QuadPart + _elapsedTime;
}
