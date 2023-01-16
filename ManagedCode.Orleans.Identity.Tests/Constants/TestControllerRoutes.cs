namespace ManagedCode.Orleans.Identity.Tests.Constants;

public static class TestControllerRoutes
{
    public const string ANONYMOUS_ROUTE = "/anonymous";
    public const string AUTHORIZE_ROUTE = "/authorize";
    public const string ADMIN_ROUTE = "/admin";
    public const string MODERATOR_ROUTE = "/moderator";
    public const string COMMON_ROUTE = "/common";

    public const string ADMIN_CONTROLLER_DEFAULT_ROUTE = "/adminController";
    public const string ADMIN_CONTROLLER_ADMINS_LIST = "/adminController/adminsList";
    public const string ADMIN_CONTROLLER_ADMIN_GET_ADMIN = "/adminController/getAdmin";
    public const string ADMIN_CONTROLLER_EDIT_ADMINS = "/adminController/editAdmin";

    public const string USER_CONTROLLER_DEFAULT_ROUTE = "/userController";
    public const string USER_CONTROLLER_ANONYMOUS_ROUTE = "/userController/anonymous";
    public const string USER_CONTROLLER_BAN_ROUTE = "/userController/ban";
    public const string USER_CONTROLLER_PUBLIC_INFO_ROUTE = "/userController/publicInfo";
    public const string USER_CONTROLLER_MODIFY = "/userController/modify";
}