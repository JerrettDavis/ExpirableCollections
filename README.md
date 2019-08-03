# ExpirableCollections
---
A basic set of collections that allow for adding items with a finite lifespan. 

# Overview
---
The collection consists of

- ExpirableDictionary
- ExpirableList

Each class replicates the functionality of the class they share their namesake with, but 
with the additional item-lifespan functionality added in. When you instantiate one of the
classes in the collection, you specify a polling interval in milliseconds and a lifespan
via a `TimeSpan` object. When the object is created, it creates a timer from `System.Timers`. 
Each time the timer is triggered, it iterates through the collection and disposes of any
expired items. 
