#include "StdAfx.h"
//#include "MetadataBase.h"
//
////template <class TId, class TMetadata>
////map<TId, shared_ptr<TMetadata>> MetadataBase<TId, TMetadata>::_cacheMap;
//
//template <class TId, class TMetadata>
//shared_ptr<TMetadata> MetadataBase<TId, TMetadata>::GetMethodMetadataBy(TId id){
//	shared_ptr<TMetadata> pMetadata ;//= _cacheMap[id];
//	return pMetadata;
//}
//
//template <class TId, class TMetadata>
//void MetadataBase<TId, TMetadata>::AddMethodMetadata(TId id, shared_ptr<TMetadata> pMethodMetadata){
//		
//	//_cacheMap[id] = pMethodMetadata;
//}
//
//template <class TId, class TMetadata>
//bool MetadataBase<TId, TMetadata>::Contains(TId id){
//	bool contains = true; //_cacheMap[id] != NULL;
//	return contains;
//}