# feature-flags-co Frontend
This is the admin site of [https://www.feature-flags.co](https://www.feature-flags.co) website, this project is used to configure your account and all the feature flags.

# Built With
- Angular
- Node

# Getting Started
These instructions will get you a copy of the project front end up and running on your local machine for development and testing purposes.

# Build and Test
Make sure you have a API instance started then modify the file **src/environments/environment.ts** by pointing the **url** property to the API.
The **projectEnvKey** property can be ommitted, we use it to point to our SaaS platform to control our features.

```
  npm install
  npm run start
```

To launch a standalone version of the whole project, including the API, Portal, Databases and analytic tools, you can use docker-compose to launch all the services. For details, please go to the FeatureFlagsCo.Docker folder under the root path of the repository and read the instructions there.

# Resources
- [Website](https://www.feature-flags.co)
- [Documentation](https://docs.feature-flags.co/)
