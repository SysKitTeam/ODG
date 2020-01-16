# ODG Provisioning Schema

## ODGTemplate

This is the root XML element.

```xml

<ODGTemplate>
    <RandomOptions />
    <Users />
    <Groups />
</ODGTemplate>

```
Available child elements for the ODGTemplate element:

Element|Type|Description
-------|----|-----------
Random Options|[Random Options](#random_options)|Section with options to automatically generate Office 365 data
Users|[Users](#users)|Section to specify users that should be created
Groups|[Groups](#groups)|Section to specify groups to be created 

## RandomOptions

Represents the root element for generating random Office 365 data. Data is populated from CSV files in `SysKit.ODG.App/SysKit.ODG.SampleData/Samples`.

```xml

  <RandomOptions>
    <NumberOfUsers />
    <NumberOfUnifiedGroups />
    <NumberOfTeams />
    <MaxNumberOfOwnersPerGroup />
    <MaxNumberOfMembersPerGroup />
  </RandomOptions>

```

Available child elements for the RandomOptions element:

Element|Type|Description
-------|----|-----------
NumberOfUsers|xs:int|Number of users that will be generated
NumberOfUnifiedGroups|xs:int|Number of Office 365 groups that will be generated
NumberOfTeams|xs:int|Number of Microsoft Teams that will be generated
MaxNumberOfOwnersPerGroup|xs:int|Maximum number of owners per generated Office 365 Group or Microsoft Team. If element is not specified, default value will be used (3)
MaxNumberOfMembersPerGroup|xs:int|Maximum number of members per generated Office 365 Group or Microsoft Team. If element is not specified, default value will be used (15)

` Important: ` If you don't specify NumberOfUsers/NumberOfUnifiedGroups/NumberOfTeams, those types of objects won't be created. 

## Users

Represents collection of user objects to be created.

```xml

  <Users>
    <User />
  </Users>

```

Available attributes for the User element:

Attibute|Type|Description
--------|----|-----------
Name|xsd:string|Username with or without the domain. This is used as a key later on to add the user as a member/owner of a group. Must be **unique**
AccountDisabled|xsd:boolean|If set to true user account will be disabled in the Azure Active Directory

## Groups

Represents collection of group objects to be created. Group types that are supported are:
- Office 365 Groups
- Microsoft Teams

```xml

  <Groups>
    <Group xsi:type="UnifiedGroup" />
    <Group xsi:type="Team" />
  </Groups>

```

### Office 365 Groups

Available attributes for the group element:

Attibute|Type|Description
--------|----|-----------
Name|xsd:string|Group mailNickname. Must be **unique**
DisplayName|xsd:string|Group display name. Two groups can have the same display name
IsPrivate|xsd:boolean|If set group visibility will be set to 'Private'

Available child elements for the group element:

Element|Type|Description
-------|----|-----------
Owners|[Owners](#owners)|Collection of users that are group owners. If element is not set, user that started ODG will be used
Members|[Members](#members)|Collection of users that are group members

### Microsoft Teams

Has the same elements and attributes as [Office 365 Group](#office-365-groups). Elements unique only to Microsoft Teams are:

Element|Type|Description
-------|----|-----------
Channels|[Channel](#channel)|Collection of standard and private channels
