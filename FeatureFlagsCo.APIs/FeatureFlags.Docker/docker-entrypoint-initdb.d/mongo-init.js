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
db.createCollection('EnvironmentUsers');
db.createCollection('EnvironmentUserProperties');