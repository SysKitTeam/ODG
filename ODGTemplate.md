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

Element|Type|Description|Optional
-------|----|-----------|--
Random Options|[Random Options](#random_options)|Section with options to automatically generate Office 365 data|Yes
Users|[Users](#users)|Section to specify users that should be created|Yes
Groups|[Groups](#groups)|Section to specify groups to be created|Yes

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

Element|Type|Description|Optional
-------|----|-----------|--
NumberOfUsers|xs:int|Number of users that will be generated|Yes
NumberOfUnifiedGroups|xs:int|Number of Office 365 groups that will be generated|Yes
NumberOfTeams|xs:int|Number of Microsoft Teams that will be generated|Yes
MaxNumberOfOwnersPerGroup|xs:int|Maximum number of owners per generated Office 365 Group or Microsoft Team. If element is not specified, default value will be used (3)|Yes
MaxNumberOfMembersPerGroup|xs:int|Maximum number of members per generated Office 365 Group or Microsoft Team. If element is not specified, default value will be used (15)|Yes

` Important:  If you don't specify NumberOfUsers/NumberOfUnifiedGroups/NumberOfTeams, those types of objects won't be created. `

## Users

Represents collection of user objects to be created.

```xml

  <Users>
    <User />
  </Users>

```

Available attributes for the User element:

Attibute|Type|Description|Optional
--------|----|-----------|--
Name|xsd:string|Username with or without the domain. This is used as a key later on to add the user as a member/owner of a group. Must be **unique** | No
AccountDisabled|xsd:boolean|If set to true user account will be disabled in the Azure Active Directory | Yes

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

Attibute|Type|Description|Optional
--------|----|-----------|--
Name|xsd:string|Group mailNickname. Must be **unique**|No
DisplayName|xsd:string|Group display name. Two groups can have the same display name|No
IsPrivate|xsd:boolean|If set group visibility will be set to 'Private'|Yes

Available child elements for the group element:

Element|Type|Description|Optional
-------|----|-----------|--
Owners|[Members Collection](#Member)|Collection of users that are group owners. If element is not set, user that started ODG will be used|Yes
Members|[Members Collection](#Member)|Collection of users that are group members|Yes

### Microsoft Teams

Has the same elements and attributes as [Office 365 Group](#office-365-groups). Elements unique only to Microsoft Teams are:

Element|Type|Description|Optional
-------|----|-----------|--
Channels|[Channel Collection](#channel)|Collection of standard and private channels|Yes

### Channel

There are 2 types of channels that can be created: **standard** and **private**. Private channels have an additional option to specify owners/members (that **must** be from the parent team membership).

```xml

  <Channels>
    <Channel />
  </Channels>

```

Available attributes for the channel element:

Attibute|Type|Description|Optional
--------|----|-----------|--
DisplayName|xsd:string|Channel name |No
IsPrivate|xsd:boolean|If set channel will be private|Yes
Owners|[Members Collection](#Member)|If channel is set to private, contains collection of users that will be owners of the private channel. **IMPORTANT:** user must be a member of the parent team. | No (if IsPrivate set to true)
Members|[Members Collection](#Member)|If channel is set to private, contains collection of users that will be members of the private channel. **IMPORTANT:** user must be a member of the parent team. | Yes

## Common Types

### Member

Element used to define membership.

Available attributes for the member element:

Attibute|Type|Description|Optional
--------|----|-----------|--
Name|xsd:string|Username with or without the domain.|No
