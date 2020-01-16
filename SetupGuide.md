# Setup Guide

To be able to run ODG you will need:
- Global Admin account
- App registration in your Azure AD

## Create a new App Registration
1. Go to your Azure AD admin center
2. Select Azure Active Directory
3. App Registrations
4. New Registration
   1. Accounts in this organizational directory only (Contoso only - Single tenant)
   2. Redirect URI -> Public client (uri can be left empty)
5. On the newly created app page go to API permissions
6. Assign:
   1. Graph API -> Delegated -> User.ReadWrite.All
   2. Graph API -> Delegated -> Group.ReadWrite.All
7. After all permissions are granted give admin consent (this may take a while to prepare)
8. Go to Manifest and change "allowPublicClient" from null to true
9. Use the Application (client) ID from the overview page

