using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewProxyStub_HTTP
{
    public enum ProtocolType
    {
        GET,
        POST,
        NOTI
    }
    public enum Protocol
    {
        REQ_JOIN=1,
        RES_JOIN,
        REQ_LOGIN,
        RES_LOGIN,
        REQ_MAKEROOM,
        RES_MAKEROOM,
        REQ_JOINROOM,
        REQ_SENDCHAT,

        REQ_ROOMLIST,
        RES_ROOMLIST,
        NOTI_CHAT
    }

    static public class ProtocolConst
    {
        public const int RESULT_SUCCESS = 200;
        public const int RESULT_REDIRECTION = 300;
        public const int RESULT_ERROR_CLIENT = 400;
        public const int RESULT_ERROR_SERVER = 500;

        public const string RESULT_SUCCESS_TEXT = "Success";
        public const string RESULT_ERROR_CLIENT_TEXT = "FAIL : Client";
        public const string RESULT_ERROR_SERVER_TEXT = "FAIL : Server";

        public const int RESULT_OK_CREATED = 201;
        public const int RESULT_FAIL_CONFLICT = 409;
        public const int RESULT_FAIL_BAD_REQUEST = 400;
        public const int RESULT_FAIL_FORBIDDEN = 403;



        public const int PTTYPE_GET_FLAG = 100;

        public const int LEN_LOGIN_ID = 20;   //ID길이
        public const int LEN_LOGIN_PASSWORD = 20; //PW길이
        public const int LEN_LOGIN_RESULT = 2;  //로그인인증값 길이
        public const int LEN_PROTOCOL_TYPE = 1;  //프로토콜타입 길이
        public const int LEN_MAX = 1000;    //최대 데이타 길이
    }
}
