﻿namespace ManagedCode.Orleans.Identity.Tests.Constants;

public static class TestControllerRoutes
{
    public const string ANONYMOUS_ROUTE = "/anonymous";
    public const string AUTHORIZE_ROUTE = "/authorize";
    public const string ADMIN_ROUTE = "/admin";
    public const string MODERATOR_ROUTE = "/moderator";
    public const string COMMON_ROUTE = "/common";

    public const string AUTH_CONTROLLER_LOGIN = "/auth/login";
    public const string AUTH_CONTROLLER_LOGOUT = "/auth/logout";
    
    public const string ADMIN_CONTROLLER_DEFAULT_ROUTE = "/adminController";
    public const string ADMIN_CONTROLLER_ADMINS_LIST = "/adminController/adminsList";
    public const string ADMIN_CONTROLLER_ADMIN_GET_ADMIN = "/adminController/getAdmin";
    public const string ADMIN_CONTROLLER_EDIT_ADMINS = "/adminController/editAdmin";

    public const string USER_CONTROLLER_DEFAULT_ROUTE = "/userController";
    public const string USER_CONTROLLER_ANONYMOUS_ROUTE = "/userController/anonymous";
    public const string USER_CONTROLLER_BAN = "/userController/ban";
    public const string USER_CONTROLLER_PUBLIC_INFO = "/userController/publicInfo";
    public const string USER_CONTROLLER_MODIFY = "/userController/modify";
    public const string USER_CONTROLLER_ADD_TO_LIST = "/userController/addToList";

    public const string PUBLIC_CONTROLLER_DEFAULT_ROUTE = "/publicController";
    public const string PUBLIC_CONTROLLER_AUTH_METHOD_ROUTE = "/publicController/authorizedMethod";
    public const string PUBLIC_CONTROLLER_ADMIN_METHOD_ROUTE = "/publicController/adminMethod";
    public const string PUBLIC_CONTROLLER_MODERATOR_METHOD_ROUTE = "/publicController/moderatorMethod";

    public const string MODERATOR_CONTROLLER_DEFAULT_ROUTE = "/moderatorController";
    public const string MODERATOR_CONTROLLER_GET_MODERATORS_ROUTE = "/moderatorController/getModerators";
    public const string MODERATOR_CONTROLLER_GET_PUBLIC_INFO_ROUTE = "/moderatorController/publicInfo";
}