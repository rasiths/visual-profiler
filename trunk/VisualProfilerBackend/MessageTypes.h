#pragma once

//Frontend -> Backend
#define	MessageType_SendMetadata 1
#define	MessageType_SendProfilingData 2
#define	MessageType_Terminate 3

//Backend -> Frontend
#define MessageType_NoData 10
#define	MessageType_AssemblyMetadata 11
#define	MessageType_ModuleMedatada 12
#define	MessageType_ClassMedatada 13
#define	MessageType_MethodMedatada 14
#define	MessageType_ProfilingData 15

#define Message_Type_TracingProfiler_Tree 31
#define Message_Type_SamplingProfiler_Tree 32

typedef UINT MessageTypes;