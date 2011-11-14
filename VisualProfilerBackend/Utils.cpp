#include "StdAfx.h"
#include "Utils.h"


void SubtractFILETIMESAndAddToResult(FILETIME * ft1, FILETIME * ft2, ULONGLONG * result ){
	ULARGE_INTEGER temp1;
	temp1.HighPart = ft1->dwHighDateTime;
	temp1.LowPart =  ft1->dwLowDateTime;
	
	ULARGE_INTEGER temp2;
	temp2.HighPart = ft2->dwHighDateTime;
	temp2.LowPart =  ft2->dwLowDateTime;
	
	*result = temp1.QuadPart - temp2.QuadPart + *result;
}
