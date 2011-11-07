#pragma once
class TypeMetadata
{
public:
	TypeMetadata(void);
	~TypeMetadata(void);

private:
	AssemblyMetadata * _pDefiningAssemblyMetadata;
};

