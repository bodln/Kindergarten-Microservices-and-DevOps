package models

import (
    "go.mongodb.org/mongo-driver/bson/primitive"
)

type Event struct {
    ID          primitive.ObjectID   `bson:"_id,omitempty" json:"id"`
    Name        string               `bson:"name" json:"name"`
    Description string               `bson:"description" json:"description"`
    Date        string               `bson:"date" json:"date"` 
    Location    string               `bson:"location" json:"location"`
    CreatedAt   primitive.DateTime   `bson:"createdAt" json:"createdAt"`
    StudentIDs  []int                `bson:"studentIds" json:"studentIds"` 
}
