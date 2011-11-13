#pragma once

class ThreadTimer
{
public:
	ThreadTimer();
	void Start();
	void GetElapsedTimeIn100NanoSeconds(ULONGLONG * elapsedTime);
	void Stop();
	void Reset();

private:
	ULONGLONG _elapsedTime;
	FILETIME _startTime;
	bool _isStopped;
	void SubtractCurrentFromStartAndAddElapsedTime(ULONGLONG * result );
};


