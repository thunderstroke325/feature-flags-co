db.createUser({
    user: 'application_user',
    pwd: 'application_pass',
    roles: [
        {
            role: 'dbOwner',
            db: 'featureflagsco',
        },
    ],
});


print('Start #################################################################');

db = db.getSiblingDB('featureflagsco');
db.createCollection('FeatureFlags');
db.FeatureFlags.createIndex({ "Id": 1 }, { unique: true });
db.FeatureFlags.createIndex({
    "EnvironmentId": 1, "FF.Name": "text", "IsArchived": 1 });
db.createCollection('EnvironmentUsers');
db.EnvironmentUsers.createIndex({ "Id": 1 }, { unique: true });
db.FeatureFlags.createIndex({
    "EnvironmentId": 1, "Name": "text", "Email": "text"
});
db.createCollection('EnvironmentUserProperties');
db.EnvironmentUserProperties.createIndex({ "Id": 1 }, { unique: true });
db.EnvironmentUserProperties.createIndex({ "EnvironmentId": 1 });
