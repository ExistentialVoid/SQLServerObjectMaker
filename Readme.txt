This library is designed to be modified toward a particular database; reducing general functionality toward specific, accurate, usage of record objects. 
This library allows users of apps (relying on said database) to avoid working with SQL and dbole objects by creating custom objects that handle SQL and SQL client lib.

The only caution to be taken in considering this methodology is that each edit of a targeted datatable must be reflected by a few (limited by design) objects.
Targeting .NET 5.0 and following OOP principles and architecture, this structure is designed to be robust and limit exceptions.
Of course futher safe practices can be built on top of the following model - whereas this model assumes close relationship between devs of library, database, and target apps.

These custom objects are expected to contain the folliwng functionality:
	1) Properties that get and set the datarecord values.
	2) Additional properties that share information about the object.
	3) A public constructor that control minimal info for an insertion (if applicable)
	4) A public constructor that looks up by primary key, and/or any other set of values
	5) The ability to view runtime exceptions (handled or otherwise)


A standard User object is a baseline for exemplification. User can be further modified while any additional objects can follow its structure.
	- All database commands are called through RecordAccessor objects
	- 'Customize Regions.cs' provides instructions for how to incorporate new datatable info necessary to estabilish custom objects
	- Additional Functions.cs are helpers

Note: IDisposable is not implemented on objects because the only unmanaged resource is on SQL connections, commands, and readers. 
	Which are all Disposed() accordingly after each use.

There is not a Database-focused object, thus connection strings are properties of the datatable objects. 
Be sure to have a standard account with updated credentials which can reliably connect.
