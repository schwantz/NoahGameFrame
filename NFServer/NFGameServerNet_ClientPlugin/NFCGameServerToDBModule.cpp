// -------------------------------------------------------------------------
//    @FileName			:    NFCGameServerNet_ClientModule.cpp
//    @Author           :    LvSheng.Huang
//    @Date             :    2013-01-02
//    @Module           :    NFCGameServerNet_ClientModule
//    @Desc             :
// -------------------------------------------------------------------------

#include "NFCGameServerToDBModule.h"
#include "NFGameServerNet_ClientPlugin.h"
#include "NFComm/NFMessageDefine/NFMsgDefine.h"
#include "NFComm/NFPluginModule/NFINetClientModule.h"
#include "NFComm/NFMessageDefine/NFProtocolDefine.hpp"

bool NFCGameServerToDBModule::Init()
{
	m_pNetClientModule = pPluginManager->FindModule<NFINetClientModule>();
	m_pKernelModule = pPluginManager->FindModule<NFIKernelModule>();
	m_pClassModule = pPluginManager->FindModule<NFIClassModule>();
	m_pElementModule = pPluginManager->FindModule<NFIElementModule>();
	m_pLogModule = pPluginManager->FindModule<NFILogModule>();
	m_pGameServerNet_ServerModule = pPluginManager->FindModule<NFIGameServerNet_ServerModule>();

	return true;
}

bool NFCGameServerToDBModule::Shut()
{

	return true;
}


bool NFCGameServerToDBModule::Execute()
{
	return true;
}

void NFCGameServerToDBModule::Register(NFINet* pNet)
{
	NF_SHARE_PTR<NFIClass> xLogicClass = m_pClassModule->GetElement(NFrame::Server::ThisName());
	if (xLogicClass)
	{
		const std::vector<std::string>& strIdList = xLogicClass->GetIDList();
		for (int i = 0; i < strIdList.size(); ++i)
		{
			const std::string& strId = strIdList[i];

			const int nServerType = m_pElementModule->GetPropertyInt32(strId, NFrame::Server::Type());
			const int nServerID = m_pElementModule->GetPropertyInt32(strId, NFrame::Server::ServerID());
			if (nServerType == NF_SERVER_TYPES::NF_ST_DB && pPluginManager->GetAppID() == nServerID)
			{
				const int nPort = m_pElementModule->GetPropertyInt32(strId, NFrame::Server::Port());
				const int nMaxConnect = m_pElementModule->GetPropertyInt32(strId, NFrame::Server::MaxOnline());
				//const int nCpus = m_pElementModule->GetPropertyInt32(strId, NFrame::Server::CpuCount());
				const std::string& strName = m_pElementModule->GetPropertyString(strId, NFrame::Server::Name());
				const std::string& strIP = m_pElementModule->GetPropertyString(strId, NFrame::Server::IP());

				NFMsg::ServerInfoReportList xMsg;
				NFMsg::ServerInfoReport* pData = xMsg.add_server_list();

				pData->set_server_id(nServerID);
				pData->set_server_name(strName);
				pData->set_server_cur_count(0);
				pData->set_server_ip(strIP);
				pData->set_server_port(nPort);
				pData->set_server_max_online(nMaxConnect);
				pData->set_server_state(NFMsg::EST_NARMAL);
				pData->set_server_type(nServerType);

				NF_SHARE_PTR<ConnectData> pServerData = m_pNetClientModule->GetServerNetInfo(pNet);
				if (pServerData)
				{
					int nTargetID = pServerData->nGameID;
					m_pNetClientModule->SendToServerByPB(nTargetID, NFMsg::EGameMsgID::EGMI_GTW_GAME_REGISTERED, xMsg);

					m_pLogModule->LogNormal(NFILogModule::NLL_INFO_NORMAL, NFGUID(0, pData->server_id()), pData->server_name(), "Register");
				}
			}
		}
	}
}

bool NFCGameServerToDBModule::AfterInit()
{
	m_pNetClientModule->AddReceiveCallBack(NF_SERVER_TYPES::NF_ST_WORLD, NFMsg::EGMI_STS_NET_INFO, this, &NFCGameServerToDBModule::OnServerInfoProcess);
	m_pNetClientModule->AddReceiveCallBack(NF_SERVER_TYPES::NF_ST_WORLD, this, &NFCGameServerToDBModule::TransPBToProxy);
	m_pNetClientModule->AddEventCallBack(NF_SERVER_TYPES::NF_ST_WORLD, this, &NFCGameServerToDBModule::OnSocketWSEvent);
	
	m_pKernelModule->AddClassCallBack(NFrame::Player::ThisName(), this, &NFCGameServerToDBModule::OnObjectClassEvent);

	m_pNetClientModule->ExpandBufferSize();

	NF_SHARE_PTR<NFIClass> xLogicClass = m_pClassModule->GetElement(NFrame::Server::ThisName());
	if (xLogicClass)
	{
		const std::vector<std::string>& strIdList = xLogicClass->GetIDList();

		const int nCurAppID = pPluginManager->GetAppID();
		std::vector<std::string>::const_iterator itr =
			std::find_if(strIdList.begin(), strIdList.end(), [&](const std::string& strConfigId)
		{
			return nCurAppID == m_pElementModule->GetPropertyInt32(strConfigId, NFrame::Server::ServerID());
		});

		if (strIdList.end() == itr)
		{
			std::ostringstream strLog;
			strLog << "Cannot find current server, AppID = " << nCurAppID;
			m_pLogModule->LogNormal(NFILogModule::NLL_ERROR_NORMAL, NULL_OBJECT, strLog, __FUNCTION__, __LINE__);
			NFASSERT(-1, "Cannot find current server", __FILE__, __FUNCTION__);
			exit(0);
		}

		const int nCurArea = m_pElementModule->GetPropertyInt32(*itr, NFrame::Server::Area());

		for (int i = 0; i < strIdList.size(); ++i)
		{
			const std::string& strId = strIdList[i];

			const int nServerType = m_pElementModule->GetPropertyInt32(strId, NFrame::Server::Type());
			const int nServerID = m_pElementModule->GetPropertyInt32(strId, NFrame::Server::ServerID());
			const int nServerArea = m_pElementModule->GetPropertyInt32(strId, NFrame::Server::Area());
			if (nServerType == NF_SERVER_TYPES::NF_ST_WORLD && nCurArea == nServerArea)
			{
				const int nPort = m_pElementModule->GetPropertyInt32(strId, NFrame::Server::Port());
				//const int nMaxConnect = m_pElementModule->GetPropertyInt32(strId, NFrame::Server::MaxOnline());
				//const int nCpus = m_pElementModule->GetPropertyInt32(strId, NFrame::Server::CpuCount());
				const std::string& strName = m_pElementModule->GetPropertyString(strId, NFrame::Server::Name());
				const std::string& strIP = m_pElementModule->GetPropertyString(strId, NFrame::Server::IP());

				ConnectData xServerData;

				xServerData.nGameID = nServerID;
				xServerData.eServerType = (NF_SERVER_TYPES)nServerType;
				xServerData.strIP = strIP;
				xServerData.nPort = nPort;
				xServerData.strName = strName;

				m_pNetClientModule->AddServer(xServerData);
			}
		}
	}

	return true;
}

void NFCGameServerToDBModule::OnServerInfoProcess(const NFSOCK nSockIndex, const int nMsgID, const char* msg, const uint32_t nLen)
{
	NFGUID nPlayerID;
	NFMsg::ServerInfoReportList xMsg;
	if (!NFINetModule::ReceivePB(nMsgID, msg, nLen, xMsg, nPlayerID))
	{
		return;
	}

	for (int i = 0; i < xMsg.server_list_size(); ++i)
	{
		const NFMsg::ServerInfoReport& xData = xMsg.server_list(i);

		//type
		ConnectData xServerData;

		xServerData.nGameID = xData.server_id();
		xServerData.strIP = xData.server_ip();
		xServerData.nPort = xData.server_port();
		xServerData.strName = xData.server_name();
		xServerData.nWorkLoad = xData.server_cur_count();
		xServerData.eServerType = (NF_SERVER_TYPES)xData.server_type();

		if (NF_SERVER_TYPES::NF_ST_WORLD == xServerData.eServerType)
		{
			m_pNetClientModule->AddServer(xServerData);
		}
	}
}

void NFCGameServerToDBModule::OnSocketWSEvent(const NFSOCK nSockIndex, const NF_NET_EVENT eEvent, NFINet* pNet)
{
	if (eEvent & NF_NET_EVENT_EOF)
	{
	}
	else if (eEvent & NF_NET_EVENT_ERROR)
	{
	}
	else if (eEvent & NF_NET_EVENT_TIMEOUT)
	{
	}
	else  if (eEvent == NF_NET_EVENT_CONNECTED)
	{
		m_pLogModule->LogNormal(NFILogModule::NLL_INFO_NORMAL, NFGUID(0, nSockIndex), "NF_NET_EVENT_CONNECTED", "connectioned success", __FUNCTION__, __LINE__);
		Register(pNet);

	}
}

int NFCGameServerToDBModule::OnObjectClassEvent(const NFGUID& self, const std::string& strClassName, const CLASS_OBJECT_EVENT eClassEvent, const NFDataList& var)
{
	if (strClassName == NFrame::Player::ThisName())
	{
		if (CLASS_OBJECT_EVENT::COE_DESTROY == eClassEvent)
		{
			SendOffline(self);
		}
		else if (CLASS_OBJECT_EVENT::COE_CREATE_FINISH == eClassEvent)
		{
			SendOnline(self);
		}
	}

	return 0;
}

void NFCGameServerToDBModule::SendOnline(const NFGUID& self)
{
	NFMsg::RoleOnlineNotify xMsg;
	const NFGUID& xGuild = m_pKernelModule->GetPropertyObject(self, NFrame::Player::GuildID());

	*xMsg.mutable_self() = NFINetModule::NFToPB(self);
	*xMsg.mutable_guild() = NFINetModule::NFToPB(xGuild);
	xMsg.set_game(pPluginManager->GetAppID());
	xMsg.set_proxy(0);
	m_pNetClientModule->SendToAllServerByPB(NF_SERVER_TYPES::NF_ST_WORLD, NFMsg::EGMI_ACK_ONLINE_NOTIFY, xMsg);
}

void NFCGameServerToDBModule::SendOffline(const NFGUID& self)
{
	NFMsg::RoleOfflineNotify xMsg;

	const NFGUID& xGuild = m_pKernelModule->GetPropertyObject(self, NFrame::Player::GuildID());

	*xMsg.mutable_self() = NFINetModule::NFToPB(self);
	*xMsg.mutable_guild() = NFINetModule::NFToPB(xGuild);
	xMsg.set_game(pPluginManager->GetAppID());
	xMsg.set_proxy(0);

	m_pNetClientModule->SendToAllServerByPB(NF_SERVER_TYPES::NF_ST_WORLD, NFMsg::EGMI_ACK_OFFLINE_NOTIFY, xMsg);

}

void NFCGameServerToDBModule::TransPBToProxy(const NFSOCK nSockIndex, const int nMsgID, const char* msg, const uint32_t nLen)
{
	NFGUID nPlayerID;
	std::string strData;
	if (!NFINetModule::ReceivePB( nMsgID, msg, nLen, strData, nPlayerID))
	{
		return;
	}

	m_pGameServerNet_ServerModule->SendMsgPBToGate(nMsgID, strData, nPlayerID);

	return;
}

void NFCGameServerToDBModule::TransmitToDB(const int nHashKey, const int nMsgID, const google::protobuf::Message& xData)
{
	m_pNetClientModule->SendSuitByPB(NF_SERVER_TYPES::NF_ST_DB, nHashKey, nMsgID, xData);
}