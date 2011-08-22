#pragma once

#include <pGinaNativeLib.h>
#include <pGinaTransactions.h>

#define ADDL_FORMAT L"[%s:%d]"
#define ADDL_ARGS __FILE__, __LINE__

#define pDEBUG(format, ...) pGina::Transactions::Log::Debug(ADDL_FORMAT format, ADDL_ARGS, ##__VA_ARGS__)
#define pWARN(format, ...) pGina::Transactions::Log::Warn(ADDL_FORMAT format, ADDL_ARGS, ##__VA_ARGS__)
#define pINFO(format, ...) pGina::Transactions::Log::Info(ADDL_FORMAT format, ADDL_ARGS, ##__VA_ARGS__)
#define pERROR(format, ...) pGina::Transactions::Log::Error(ADDL_FORMAT format, ADDL_ARGS, ##__VA_ARGS__)