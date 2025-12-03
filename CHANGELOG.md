## [1.9.0](https://github.com/see-paw/backend/compare/v1.8.0...v1.9.0) (2025-12-03)


### Features

* **WebAPI:** Add conditional NGROK URL support based on environment variable ([510ccae](https://github.com/see-paw/backend/commit/510ccaecbe1af6d950e947509237416c349a20f1))
* **WebAPI:** Add Testing environment support and database initialization logic ([ef90a3d](https://github.com/see-paw/backend/commit/ef90a3dee07ac752bd5d005d942ef41ab936ad2f))

## [1.8.0](https://github.com/see-paw/backend/compare/v1.7.0...v1.8.0) (2025-12-02)


### Features

* **DbInitializer:** Add seed data for owned animal testing ([948c59c](https://github.com/see-paw/backend/commit/948c59c7b0a34bcec7dd5a627c3ad88bdcc9889a))

## [1.7.0](https://github.com/see-paw/backend/compare/v1.6.0...v1.7.0) (2025-12-02)


### Features

* updated getAnimalDetails to return the sum of the amount of the active fosterings of the animal ([644f3f1](https://github.com/see-paw/backend/commit/644f3f11fd352f6a8338df3f6bc268bb53dc132f))

## [1.6.0](https://github.com/see-paw/backend/compare/v1.5.1...v1.6.0) (2025-12-01)


### Features

* **Schedule:** Enhance schedule handling with additional ownership checks and activity slot creation ([8809cd0](https://github.com/see-paw/backend/commit/8809cd0c14451581bcf72a7bcadbedf23551c415))


### Code Refactoring

* **Tests:** Remove redundant test cases and ensure breed entity is added in scheduling tests ([621e898](https://github.com/see-paw/backend/commit/621e898e619414b0a44f89c904e48b68b226796b))

## [1.5.1](https://github.com/see-paw/backend/compare/v1.5.0...v1.5.1) (2025-11-27)


### Bug Fixes

* add filters to the endpoint getAnimalsByShelter, changed BreedDTO so it can have an Id, changed ReqCreateAnimalDTO so it can have an Id, changed ResCurrentUserDto so that can have ShelterName ([0a108ec](https://github.com/see-paw/backend/commit/0a108ecdee0325f4d26e3f60af3612e24b2aa428))

## [1.5.0](https://github.com/see-paw/backend/compare/v1.4.2...v1.5.0) (2025-11-26)


### Features

* add endpoint to list detailed shelter information by shelter id, requires auth ([0f09ac5](https://github.com/see-paw/backend/commit/0f09ac5d6e58890dbaa63af43e4fa0864254a52a))


### Chores

* Refactor Docker image tagging in workflow ([6817820](https://github.com/see-paw/backend/commit/681782053ddac0fdd09c1a1c8273120c354e5ed5))

## [1.4.2](https://github.com/see-paw/backend/compare/v1.4.1...v1.4.2) (2025-11-24)


### Chores

* Refactor Docker image tagging in workflow ([d20c408](https://github.com/see-paw/backend/commit/d20c4085fbcdd0b9be0f9c85eed3280a26213d8a))

## [1.4.1](https://github.com/see-paw/backend/compare/v1.4.0...v1.4.1) (2025-11-22)


### Documentation

* **Users:** Added documentation to the new /users/me endpoint ([4244dea](https://github.com/see-paw/backend/commit/4244dea077567dce6e332215a7a929bd077dcb59))

## [1.4.0](https://github.com/see-paw/backend/compare/v1.3.1...v1.4.0) (2025-11-22)


### Features

* **Users:** add GET /api/users/me endpoint to retrieve authenticated user info ([1e4ffa9](https://github.com/see-paw/backend/commit/1e4ffa926cbff03b618aa9937a07376229e40d14))

## [1.3.1](https://github.com/see-paw/backend/compare/v1.3.0...v1.3.1) (2025-11-21)


### Bug Fixes

* add real images to animals ([153dd9b](https://github.com/see-paw/backend/commit/153dd9b6f12e367c772f05b3856aa62aa7fd993c))
* change progem.cs so only use one seed ata(DbInitializer) ([abf91bd](https://github.com/see-paw/backend/commit/abf91bd8a28c1875c29bfd11879894cbae93bf9f))
* change program.cs to have two seed data, one for the backend tests and another for the frontend tests-have to test with the the frontend pipeline ([36fe738](https://github.com/see-paw/backend/commit/36fe7389b9528c4f96970aba3d25c10d0fc135df))
* tests that failed because of the new animals images ([ee2a59b](https://github.com/see-paw/backend/commit/ee2a59bbe125112b163b86124fb629b4236d1841))

## [1.3.0](https://github.com/see-paw/backend/compare/v1.2.5...v1.3.0) (2025-11-19)


### Features

* add GetUserRole and GetUserId endpoints ([9b060e1](https://github.com/see-paw/backend/commit/9b060e1611cb9189fe876b02167445d86b4f9230))


### Chores

* add swagger ui to program.cs ([0c8b3ab](https://github.com/see-paw/backend/commit/0c8b3abbd10a0ce8e7fd580fd1281dc50e4b3b6e))
* remove empty file ([56845de](https://github.com/see-paw/backend/commit/56845de4f0ae16b7657813442c0c2a463056a184))

## [1.2.5](https://github.com/see-paw/backend/compare/v1.2.4...v1.2.5) (2025-11-15)


### Chores

* Added http port to backend ([9ee5430](https://github.com/see-paw/backend/commit/9ee54306ef7766c1a90a88dc8286df7e5dcb0caa))

## [1.2.4](https://github.com/see-paw/backend/compare/v1.2.3...v1.2.4) (2025-11-15)


### Code Refactoring

* Eliminated health endpoint ([5903f12](https://github.com/see-paw/backend/commit/5903f12e106b15ad7517e59a89a340a4ca1a38ef))

## [1.2.3](https://github.com/see-paw/backend/compare/v1.2.2...v1.2.3) (2025-11-15)


### Chores

* Added health endpoint ([04f9319](https://github.com/see-paw/backend/commit/04f931994502f1058edfe3d8d0662648ab7d2722))

## [1.2.2](https://github.com/see-paw/backend/compare/v1.2.1...v1.2.2) (2025-11-12)


### Bug Fixes

* **Pagination:** Fixed Paginated return data ([e2750d3](https://github.com/see-paw/backend/commit/e2750d3aaf8c647be9d8aea3ff70d76f7225cbfd))

## [1.2.1](https://github.com/see-paw/backend/compare/v1.2.0...v1.2.1) (2025-11-12)


### CI/CD

* **docker:** add workflow to publish docker image to github container registry ([f326657](https://github.com/see-paw/backend/commit/f326657efcbf1b85e006b76ce49e863aceb9fe2a))
* **docker:** Merge pull request [#175](https://github.com/see-paw/backend/issues/175) from see-paw/develop ([8e6d959](https://github.com/see-paw/backend/commit/8e6d95938e5b2a5ce969217c37d113f676f740bd))


### Chores

* Delete v1.1.0 ([3135084](https://github.com/see-paw/backend/commit/3135084f7753e08f5801bb2223481efe24e0ee7e))

## [1.2.0](https://github.com/see-paw/backend/compare/v1.1.0...v1.2.0) (2025-11-07)


### Features

* add command and controller to create a fostering activity and made the unit tests for it. add new entities for slots ([7bc5e46](https://github.com/see-paw/backend/commit/7bc5e4663d5dc32751ee17c014eedf5de3ac4b9a))
* add notification to shelter admin when new ownership activity is created ([9ea8e4c](https://github.com/see-paw/backend/commit/9ea8e4ce690ed74aa9a05f583addc4ad63e1c68c))
* add notifications endpoints to retrieve data when user logs in ([2d0a29c](https://github.com/see-paw/backend/commit/2d0a29c8f64b1d9df2484016d3bcb92d7d342671))
* add notifications to relevant commands ([e157067](https://github.com/see-paw/backend/commit/e1570677738179e7315e7aa49c4a8e3efd579994))
* added cancel fostering activity feature, and all the tests needed ([b830723](https://github.com/see-paw/backend/commit/b8307238cbcb0b47f8cb0b9f2eb1d1aac86562e2))
* added new unit tests and documentation; also added roles to program.cs ([ca95cf7](https://github.com/see-paw/backend/commit/ca95cf760d034c05b62c5a146f59746b2b9e5ea9))
* added new unit tests to createAnimal and EditUserProfilePage ([53f916c](https://github.com/see-paw/backend/commit/53f916c4204ac33e601bb1a843c4d50da1ffb21e))
* added user register controller, handler, dto and validator ([a2556db](https://github.com/see-paw/backend/commit/a2556db70f68f1cd145f220aa8a7072559ac0ddb))
* addFavorite and deactivateFavorite controller, handler, unit tests and documentation ([f02dd78](https://github.com/see-paw/backend/commit/f02dd7888ca0da44af63f4968e554d53f7c9f253))
* **alerts:** add ownership activity completion background task ([1920258](https://github.com/see-paw/backend/commit/1920258837732141d7ff88b8069298f54ef4cf8b))
* **alerts:** add reminder service and ownership activity completion background task ([7d7e4f2](https://github.com/see-paw/backend/commit/7d7e4f22d411e95af19f596a4359f813e296df49))
* **Animals:** Implemented Get Animals with AND Filters ([2048b5a](https://github.com/see-paw/backend/commit/2048b5a02d89584974cae037fe9db0742f7162d7))
* **Domain:** Added Slot Entity to Data Model ([1527af9](https://github.com/see-paw/backend/commit/1527af942311a6e5c693d6d995d26bb65796ba7c))
* **Domain:** Added Slot Entity to Data Model ([f8a872e](https://github.com/see-paw/backend/commit/f8a872e193970e2dd475bd51d95194a2c2fede12))
* **notifications:** add SignalR notification system ([935f1d3](https://github.com/see-paw/backend/commit/935f1d3c9188bc4b512ce73ffd32dd203b2b93b5))
* **ownershipRequests:** add notification to AdminCAA of new ownership requests creation ([2a4e9a8](https://github.com/see-paw/backend/commit/2a4e9a8e7adcc7d7ad245445de828729f9e2caa1))
* refactor account controller and register handler and added unit tests ([949c2ed](https://github.com/see-paw/backend/commit/949c2ed840a3813f97e166f891b2885afcb77008))
* **Scheduling:** Implemented Get Animal Weekly Schedule, without slot clipping ([be5e2be](https://github.com/see-paw/backend/commit/be5e2bec2828462657affc2b4c7bae308a6728e5))
* swagger ([a9ed6d2](https://github.com/see-paw/backend/commit/a9ed6d275a148083e67018cb18282e05de5e6e1a))
* updated AccountController, handler and dto to register an Admin CAA account ([7de3a18](https://github.com/see-paw/backend/commit/7de3a18f44030ec559d035c5503dd90a511e0115))
* updated getAnimalsList handler and controller to also be sorted by name, age and createdAt asc and desc. Also added new unit test to getAnimalsListHandlerTests ([b3e454d](https://github.com/see-paw/backend/commit/b3e454d5531cde59c5b795f26b03bac68b9b3c00))


### Bug Fixes

* added a mapper for the new dto ([3ab971f](https://github.com/see-paw/backend/commit/3ab971fa78ceddfd88b4612eac47351a00995697))
* added missing validations in cancel fostering ([ff3e1d3](https://github.com/see-paw/backend/commit/ff3e1d3ab8b5b72eb87c5824f2443db803763959))
* **approveOwnershipRequest:** add active fosterings list before they are cancelled ([7d096f8](https://github.com/see-paw/backend/commit/7d096f846b0cd7b7246896f8a24821f625b27f15))
* change the result from the command CreateFosteringActivity, change the controller tests ([479338a](https://github.com/see-paw/backend/commit/479338a2227f062bfae5a19b78d5c6827c5bbb42))
* create activity handler test ([e9538aa](https://github.com/see-paw/backend/commit/e9538aa0ba02da6385226fcbc58160773b888e2e))
* **Filters:** Fixed age spec logic ([e6a3019](https://github.com/see-paw/backend/commit/e6a3019bbb22f0b83d633bb878237a6075547ed0))
* **notificationsController:** change to http endpoints' authorization ([63b2b4a](https://github.com/see-paw/backend/commit/63b2b4a2a9a3db3e76a2ca5cdd6dc3d509898d65))
* **tests:** updated tests because of merge with develop ([3635df3](https://github.com/see-paw/backend/commit/3635df3272b7b5c96fa8f1d3a387c6f46ff359ca))
* update dbinitializer tests ([8d1db59](https://github.com/see-paw/backend/commit/8d1db59f9926ecc194d04e4c0c738de1aa329edd))


### Documentation

* **Filters:** Added missing documentation ([616aea4](https://github.com/see-paw/backend/commit/616aea434697a3de67b6195dd3c265dcbe3e22b2))
* **Scheduling:** Added Documentation for Scheduling files ([509491b](https://github.com/see-paw/backend/commit/509491bf91e8b7077545bbae8601d6ecc49838e5)), closes [#75](https://github.com/see-paw/backend/issues/75)


### Code Refactoring

* **backgroundTasks:** created baseActivityReminder superclass to avoid repeating code ([14695c7](https://github.com/see-paw/backend/commit/14695c768420a9d23c8e3a370876e8293ebe69e0))
* baseActivityReminderTask to have portuguese messages to users ([807d6be](https://github.com/see-paw/backend/commit/807d6be5467941db2aa6a6daf6b249ebc131d165))
* Project Cleanup ([12ed324](https://github.com/see-paw/backend/commit/12ed324f5240ca99f867b2a9662192071b5ae77c))
* **Roles:** Added AppRoles Domain static class ([a1c23fd](https://github.com/see-paw/backend/commit/a1c23fda9f20318e43c8db1e50596a24837fc9ca))
* **Scheduling:** Added multi-day Slot clipping and other validations ([d5d59b7](https://github.com/see-paw/backend/commit/d5d59b73c4559f17700c00361a2494ca62984274))
* **Seed:** Added seed data for Scheduling testing ([4046c08](https://github.com/see-paw/backend/commit/4046c088bb1665d82646da07dd75b134ba6c2dc0))
* **Seed:** Initialized Refactoring Data Seed ([7e44d84](https://github.com/see-paw/backend/commit/7e44d8494b9279d94e3547782c22be381930ddbb))


### Tests

* add unit tests ([1991cae](https://github.com/see-paw/backend/commit/1991cae5693a6796f55157d5af46e3cc7e8eaa17))
* **alerts:** add integration tests for alerts/reminders system ([429f032](https://github.com/see-paw/backend/commit/429f0320c06a5ae8d29c129d03d41691662460ba))
* **Animals:** Added Get Animals with Filters Unit Tests ([1a12b16](https://github.com/see-paw/backend/commit/1a12b16abc39797b5f1aea037dba8a5768c0bf7e))
* **notifications:** add unit tests in the respective handlers to test notifications ([85ad30b](https://github.com/see-paw/backend/commit/85ad30b8a0777e0fbd777efbb0dd752c803a28aa))
* **Scheduling:** Added unit and integration tests for Get Animal Weekly Schedule ([d94d04c](https://github.com/see-paw/backend/commit/d94d04c390cebb42b53ce5ae6e7e33e0d106d468))


### Chores

* apply formatting styles ([0e8e5cb](https://github.com/see-paw/backend/commit/0e8e5cbe8b23c254c7423c77286af7ee6032443a))
* as a test, changed pipeline to ignore integration tests ([5e1f822](https://github.com/see-paw/backend/commit/5e1f8229e4f204c1ca104f8e9302639a72b3fdb1))
* change Hubs location to respect architecture construction and add services to program.cs ([414e3d9](https://github.com/see-paw/backend/commit/414e3d9c824edb387ba0112ce2264f007103c810))
* refactor files and folders ([52fbe8c](https://github.com/see-paw/backend/commit/52fbe8cd1c5ea3f39d67de408cad527989723049))
* remove warnings from build ([62a24ef](https://github.com/see-paw/backend/commit/62a24ef50c038df95907e559a3aae6036a837c7e))
* there's only one OwnershipController now ([bce1f7e](https://github.com/see-paw/backend/commit/bce1f7e236ebdac9fcce14fcdc0148b278077874))
* update dbinitializer for separate notifications test data ([496e7b3](https://github.com/see-paw/backend/commit/496e7b31b4c93b1396fa8d3336a709bd0af3888a))
* update from develop ([1e83a3a](https://github.com/see-paw/backend/commit/1e83a3aa3ff0d019e8c4e53a418d1d3deffe6fac))
* updated FavoritesController ([8c56bef](https://github.com/see-paw/backend/commit/8c56befbcdbf92069859f7dcaab481b3ddf32d64))
* updated pagedList class and fixed unit tests; reorganized unit tests ([5df7c0e](https://github.com/see-paw/backend/commit/5df7c0e64ddbcfc54f694e1b59a644ea6bc9d38b))

## [1.1.0](https://github.com/see-paw/backend/compare/v1.0.4...v1.1.0) (2025-11-02)


### Features

* **activities:** add create ownership activity and get ownership activities with filtering ([3b341c7](https://github.com/see-paw/backend/commit/3b341c7c4ee462011ef10ee090f48e58b22b83bc))
* **activities:** add endpoint to cancel activity with unit tests ([dd80321](https://github.com/see-paw/backend/commit/dd8032173508ade629d94bb5feab046620db69f3))
* add ability to seed in production mode, only if DB is empty ([ef203fb](https://github.com/see-paw/backend/commit/ef203fbdeb84b8c7a0f0c7d56a5c0dc922ec9cd2))
* add activities scheduling ([8443461](https://github.com/see-paw/backend/commit/8443461c909c7595b0d82d551571d2a2d2f16428))
* add initial data ([5cbf528](https://github.com/see-paw/backend/commit/5cbf528c69acc3d0b48658b327cecdc9de6e2e26))
* add seed data and check the integration tests ([7cc0a18](https://github.com/see-paw/backend/commit/7cc0a1831ffdf183079506a6f564cacee0ceffe6))
* added auth to CreateAnimal and GetAnimalsByShelter ([c2ff17e](https://github.com/see-paw/backend/commit/c2ff17ed1cc9ac60ee1b5822e720be90c902e53b))
* added auth to CreateAnimal and GetAnimalsByShelter ([6b69bdb](https://github.com/see-paw/backend/commit/6b69bdb51e7f8e57ce3f00ca12c2e9d9c68db21c))
* Added custom response messages for invalid authorization ([d90f0ba](https://github.com/see-paw/backend/commit/d90f0bae8582619bd18bbec6df2420457d2ac64c))
* added DeactivatedAnimal feature, documentation and unit tests ([dcbf2d0](https://github.com/see-paw/backend/commit/dcbf2d0ae1fe2798101149b3a39d521b020ea179))
* Added error message for unauthorized and forbidden users. ([86cbe3e](https://github.com/see-paw/backend/commit/86cbe3ed7c541c5357621724847cbbca1fc3e787))
* added images for animals and shelters ([baf9843](https://github.com/see-paw/backend/commit/baf98436f7bbbcd364f235b88ba3b0fd865d3602))
* Added Infrastructure Project and IUserAccessor for getting hold of the current user logged in. ([378a2d8](https://github.com/see-paw/backend/commit/378a2d8ff69eb72a229c3c7f7527b92c7227148b))
* added unit tests ([35daf8d](https://github.com/see-paw/backend/commit/35daf8d2392476547b4c4c468ed2e1e9d9344afe))
* **Animal:** Refactored EditAnimal endpoint ([b6d2ee5](https://github.com/see-paw/backend/commit/b6d2ee5ce21401da2ae149fc44f0641350578d13))
* Configured Identity with JWT ([4225b3b](https://github.com/see-paw/backend/commit/4225b3b2aa2b7d62394aa99bb54afc1bf1a63cd9))
* delete animal and documentation ([ec03380](https://github.com/see-paw/backend/commit/ec033800c315e61d912ecb632a379b8de81c61d9))
* edit animal dto and validator ([da81b58](https://github.com/see-paw/backend/commit/da81b58ef0a46eb780221c87a27fddff574b0e48))
* **favorites:** add initial GET structure for user favorites ([2cabad5](https://github.com/see-paw/backend/commit/2cabad5419a0c4bc9907399cde4384cc12acfc04))
* **Fostering:** Implemented AddFostering Controller and Handler ([b9e6279](https://github.com/see-paw/backend/commit/b9e627987b91c3d19b8c7786ef3400c2f48d96e7))
* **Fostering:** Initialized AddFostering Controller ([5774c9c](https://github.com/see-paw/backend/commit/5774c9c8e095e3c6fbea8cab75a343e83a5ac66b))
* get and edit users profile ([bd57bcc](https://github.com/see-paw/backend/commit/bd57bcc4aa474ec0354bd8e57881a48e7020a3d1))
* getActiveFosterings and CancelFostering ([cc5f40c](https://github.com/see-paw/backend/commit/cc5f40c60dc3be99e5c1d30a0fad54e3c670d4c4))
* **Image:** Added Cloudinary Image Id ([e682cc7](https://github.com/see-paw/backend/commit/e682cc7c4980f6481ad9f73c45dea4531ffaf7f0))
* **Image:** Implemented Generic AddPhoto MediatR class ([296622f](https://github.com/see-paw/backend/commit/296622fa47770cdc6e1ab28257afaf1db38b4ea4))
* **Image:** Integrated Cloudinary Images ([a4456e6](https://github.com/see-paw/backend/commit/a4456e6f2e2f9f4400bae951eec5738c7fb2b3d7))
* **ownership:** add ownership requests API endpoints ([e483a88](https://github.com/see-paw/backend/commit/e483a889eea95c37a95a07733dda86699ff0c3ba))
* **ownershipRequests:** add new GET endpoint to list ORs by shelter ([4635639](https://github.com/see-paw/backend/commit/4635639b142a18a05414d02a2fec8b45bd904835))
* **ownershipRequests:** add ownership request lifecycle commands ([56be3cb](https://github.com/see-paw/backend/commit/56be3cb8cb2c27b890e34eeac58148a5248f2126))
* **ownershipRequests:** add validators for creating, rejecting and updating ([1a46021](https://github.com/see-paw/backend/commit/1a460213f47148979cc958c10e3a50f74f4cf8b4))
* refactor editUserProfile, unit tests and documentation for all features ([a111a41](https://github.com/see-paw/backend/commit/a111a4181d5e9ed691e3ad0dcc3c28c2cc216df5))
* updated CreateAnimal, getAnimalsList and getAnimalsByShelter ([5929cab](https://github.com/see-paw/backend/commit/5929caba2f86ce83edf3bf519fa2f84bdd174043))
* updated CreateAnimalValidator and CreateAnimalsTests ([aeddc83](https://github.com/see-paw/backend/commit/aeddc830be073cba70f102996f1ba62dab1cc7de))


### Bug Fixes

*  Fixed some bugs ([be05ba8](https://github.com/see-paw/backend/commit/be05ba8418b62b1582a2ab1f443764cfb1ee2cbc))
* **activities:** stop new activity from being created when previous activity exists ([c68cb6e](https://github.com/see-paw/backend/commit/c68cb6e288dfef62bcb31d3a72bfa6c62a850153))
* Added Integration Test fixes ([c5d11b3](https://github.com/see-paw/backend/commit/c5d11b301b62816417b78a4026a4735909c08fdd))
* dbinitializer test ([27cf350](https://github.com/see-paw/backend/commit/27cf350891b0a6bdc98196897c15ab61b7af0886))
* **domain:** changed some parameters from set to init in Animal.cs ([6ec43c2](https://github.com/see-paw/backend/commit/6ec43c27749dd752c6381fb8a492edec6821fda4))
* Fixed merge problems ([dd736df](https://github.com/see-paw/backend/commit/dd736df68bea2a9c3890aaec2d42b3f5fbeac045))
* Fixed merge problems ([eeb240f](https://github.com/see-paw/backend/commit/eeb240ff0b85559fcd13480e5d7d46b7658695ca))
* Fixed wrong middleware order (IdentityResponseMiddleware before ExceptionMiddleware) ([9e7f885](https://github.com/see-paw/backend/commit/9e7f88557f76c60b6b8bb45990a41cfb17eda39c))
* **Fostering:** Fixed bugs in Add Fostering logic ([c3e3b29](https://github.com/see-paw/backend/commit/c3e3b29f6fdebf1653c839c592760113c8a10c4c))
* merge with develop branch, correct issues, modify checkEligibility to allow fostered animals to be adopt ([533c95e](https://github.com/see-paw/backend/commit/533c95e15a5cdc05e8a0e3019de7c0ebcf06cc90))
* **persistence:** removed duplicate rule for shelter images ([e1ac9cd](https://github.com/see-paw/backend/commit/e1ac9cdef33e5dba9ac7b2f1b38416d96ef8ea65))
* Reinitialized migrations ([d5b3f4e](https://github.com/see-paw/backend/commit/d5b3f4ee973710f662fcb16cc1ec0d35acf0439e))
* Reinitialized migrations ([0963b36](https://github.com/see-paw/backend/commit/0963b36b5d1c36b247d2f971734e530f15ab55c8))
* Resetted migrations and fixed automapper configurations ([877a124](https://github.com/see-paw/backend/commit/877a1240559a7fd072f6b1a51653b3592ea8fdec))
* Resolved some issues in the pipeline ([a8959b0](https://github.com/see-paw/backend/commit/a8959b0f91eff464993257009a26ce574a74a96b))
* **tests:** refactored ownership request controller tests ([a50307b](https://github.com/see-paw/backend/commit/a50307bfb5bf924ca82265e4d9741dd80f7434c1))
* typo in tests ([f938084](https://github.com/see-paw/backend/commit/f9380840942148a55f0ca348ee171be04878fabb))
* unit test ([064b382](https://github.com/see-paw/backend/commit/064b3822c3d95cce526ab6221853da6ad98c9174))


### Documentation

*  Added Documentation ([c7ffcd7](https://github.com/see-paw/backend/commit/c7ffcd732cdf2e52bf09fb11b024a2957c17ceeb))
* Added documentation ([de19d1b](https://github.com/see-paw/backend/commit/de19d1bbf8cbc19c4906a520007c13bc2ca85c8c))
* added documentation for my features ([ef65ce8](https://github.com/see-paw/backend/commit/ef65ce8e1337049b0c1411988f158d64d1bae582))
* **Fostering:** Added documentation for Add Fostering ([28a25a7](https://github.com/see-paw/backend/commit/28a25a7ac4111e49e27fe9e7bd66acd19feeeafe)), closes [#63](https://github.com/see-paw/backend/issues/63)


### Code Refactoring

* **Animal / Image:** Refactored CreateAnimal and AddImagesToAnimal ([4c48378](https://github.com/see-paw/backend/commit/4c483780d361ca46b5fab9c7dc6fbef240c95b6f))
* **Fostering:** Refactored Add Fostering logic to FosteringService ([7c761a8](https://github.com/see-paw/backend/commit/7c761a8b3a462c97d76d98fbcb6c2d2eca24ed2e))
* **Image:** Refactored Image Endpoint logic ([64f3759](https://github.com/see-paw/backend/commit/64f3759571c2006610e8c7dc9b33321eff41ddbb))


### Tests

* add more entries to dbinitializer ([94a0d52](https://github.com/see-paw/backend/commit/94a0d520d07f12e8f57d1a6721207e5bbb098fa4))
* Added new tests for DbInitializer ([5bd391a](https://github.com/see-paw/backend/commit/5bd391adf1e9d45f7dfffa7b6b784a8f6c7030a8))
* Added new tests for UserAccessor ([82eab56](https://github.com/see-paw/backend/commit/82eab56d5cf20063802e6287eb7d679b961284ad))
* **favorites:** add unit tests to getUserFavorites query ([90c7870](https://github.com/see-paw/backend/commit/90c787037f827f3a0fb78b5b686a74e30bb9d0fa))
* **Fostering:** Added Unit Tests for AddFostering Handler ([d591412](https://github.com/see-paw/backend/commit/d591412de266f7e8db17e5a7540cb8c0c220870f))
* **Images / Animals:** Refactored Animal tests and implemented Tests for the new Image endpoints ([3d37d32](https://github.com/see-paw/backend/commit/3d37d329e7698c00fe9d1d6c3805e3cb6cb82129))
* **ownershipRequests:** add unit tests ([86caa42](https://github.com/see-paw/backend/commit/86caa427d55399cd504a1c48dc9415eca295062a))
* **ownershipRequests:** add validators unit tests ([7b47a67](https://github.com/see-paw/backend/commit/7b47a6781150822960c69efecb81431b21483650))
* **ownershipRequests:** refactor controller tests to have only one assert per test ([24c1fce](https://github.com/see-paw/backend/commit/24c1fcefc02992b90db85707afb997e482fb8df1))
* tested createAnimals, getAnimalsList and getAnimalsByShelter ([e72307b](https://github.com/see-paw/backend/commit/e72307b840773797ec1af00ec3bbdcdbba7d0c4c))


### CI/CD

* added pipeline for local runner ([0c4419f](https://github.com/see-paw/backend/commit/0c4419f6df3b91b67017ffb2294e83b9a3d0242a))
* added pipeline for local runner ([b33901e](https://github.com/see-paw/backend/commit/b33901e5a124967b811f164028c8c9a8a381fc75))
* added sleep to lauch db ([5c6d0c7](https://github.com/see-paw/backend/commit/5c6d0c7a256a4903b2ae607c865c1e11e46a2db5))
* **Images:** Configured Postman working directory for getting files ([3049b59](https://github.com/see-paw/backend/commit/3049b5907a66db8e42d2771843f3554a3d135370))


### Chores

* add baseapicontroller from dev for testing ([55a8e3c](https://github.com/see-paw/backend/commit/55a8e3c0bced00ff12d3730d22cfba6a9692702a))
* add new migration ([aa0fa9e](https://github.com/see-paw/backend/commit/aa0fa9ef938acb546306712bd4fb0cb889c32d36))
* add new migration ([c7281c0](https://github.com/see-paw/backend/commit/c7281c0bde7110cc1707388f0b0c7803e34b1eb7))
* add new user to dbinit ([a9803c9](https://github.com/see-paw/backend/commit/a9803c957f72bf5845943f8576cb048e07809a92))
* add test coverage report to .gitignore ([984e95e](https://github.com/see-paw/backend/commit/984e95e27d9eca6326caf79ce46cb7bb538db491))
* added comment to tests ([37780a2](https://github.com/see-paw/backend/commit/37780a27ae19431638d73d44964127958e492f6c))
* changed main pipeline to run on PRs to the main branch ([5d40104](https://github.com/see-paw/backend/commit/5d401047e1998e2c4a29f9b405240e661dd7c0a9))
* fixed a test ([d1a1028](https://github.com/see-paw/backend/commit/d1a1028850b130532ee05137d77b7cad084e7bda))
* fixed build problems ([a759e4f](https://github.com/see-paw/backend/commit/a759e4fcee10b4a97ffa39f71c6f40aa8f68a0ac))
* fixed merge ([d21a30b](https://github.com/see-paw/backend/commit/d21a30bb4df17bfa6b27eb6698929ff16da773a2))
* fixed merge ([fd74361](https://github.com/see-paw/backend/commit/fd743610b9ba1efb708583211378865bbe781140))
* fixing merge ([eba00ff](https://github.com/see-paw/backend/commit/eba00ff43a93d1c8a3d3288bfb22c90ea5113187))
* merged develop into current branch for update ([1b27680](https://github.com/see-paw/backend/commit/1b27680c7c39aa3f03fe5bc56b7588935cfb7216))
* merged with a more recent branch ([bb1b74c](https://github.com/see-paw/backend/commit/bb1b74c689c26200aa379f108b3b589619feb66c))
* refactor animalsController and sheltersController ([575127c](https://github.com/see-paw/backend/commit/575127c6f774c27e9828763aa8f0e4c00a20b98f))
* refactor createAnimal and AnimalsController ([2d7ce27](https://github.com/see-paw/backend/commit/2d7ce27cf0dbed8d710f3d9d183c3f71ccaa088f))
* refactor getAnimalsByShelter ([57db411](https://github.com/see-paw/backend/commit/57db41161401634d1c06947c37e83e3eb98f10b7))
* refactor getAnimalsByShelter ([7248edd](https://github.com/see-paw/backend/commit/7248edd252b08ac35f500d04bbb46912102461b2))
* refactor GetAnimalsByShelter ([47f8a6d](https://github.com/see-paw/backend/commit/47f8a6de7682976e0cf42377ecfbd53b29cb82b6))
* refactor unitTests ([865d8c7](https://github.com/see-paw/backend/commit/865d8c7b6d1c0c7c39e6e32c3c6adc9e130ab22e))
* release production update ([eb8162c](https://github.com/see-paw/backend/commit/eb8162c698b2e1f8a6cd2bb3f8af91cf6b4ce4ec))
* remove old code ([bab2e0d](https://github.com/see-paw/backend/commit/bab2e0d7531f1472bf0b78d85f0f1c7be50a2c5d))
* resolving conflits ([c105911](https://github.com/see-paw/backend/commit/c1059117f4d2efb1892bc677e12a271a9ee9efc7))
* trigger CI pipeline ([fbe98f1](https://github.com/see-paw/backend/commit/fbe98f146bce407a317020c0b31db4591b6a86ab))
* trigger pipeline ([155f7d1](https://github.com/see-paw/backend/commit/155f7d19b02e6839829e155d2982386ec54551b6))
* trigger pipeline ([c2a7fd0](https://github.com/see-paw/backend/commit/c2a7fd0e0413d2b4cdd371360eadda0ad22c247e))
* update from dev ([53794e0](https://github.com/see-paw/backend/commit/53794e0b8a574eaa6e1d3b89a322c4ee24b2d4b9))
* updated DTOs files' name ([39fa5b9](https://github.com/see-paw/backend/commit/39fa5b9c3ea3cc36c817348d4c7071a578c24889))
* updated EditAnimalController ([129a812](https://github.com/see-paw/backend/commit/129a81288c697c284f2a26adc6e6b87265e89b98))
* updated MappingProfile ([45e3eed](https://github.com/see-paw/backend/commit/45e3eed5239c9078ef4de46d870d49cec62a859b))
* updated pageSize in getAnimalsList ([ed20527](https://github.com/see-paw/backend/commit/ed20527d4050d7503cf7df3fb3e482eec8839c68))
* updated seed ([dc268f9](https://github.com/see-paw/backend/commit/dc268f9d016cd41fb7faa48e3e807e2210e899a2))
* updated test files ([6968807](https://github.com/see-paw/backend/commit/6968807f484ce44386c0379d26aba04579545cce))
* updated Tests.csproj ([d960f6a](https://github.com/see-paw/backend/commit/d960f6ace8144e08ffccb0b77a9fe5a352ed1680))
* updated with develop branch ([994c3d4](https://github.com/see-paw/backend/commit/994c3d4f286125b3ce6cacc9c7444c076e3e5f23))

## [1.0.4](https://github.com/see-paw/backend/compare/v1.0.3...v1.0.4) (2025-10-31)


### Chores

* Enhance release workflow with tag fetching ([ae02ea7](https://github.com/see-paw/backend/commit/ae02ea7cd0680ce22abc94ce09347c9db030630f))

## 1.0.0 (2025-10-31)


### Chores

* Adapted new Database schema ([c879463](https://github.com/see-paw/backend/commit/c87946340d4c31d99e2af7949add04b681eec2a3))
* Add .releaserc.json for semantic release configuration ([0612143](https://github.com/see-paw/backend/commit/061214328be536d061a7a084d11a7236eb8417b5))
* add azure configuration for CD ([16af7fa](https://github.com/see-paw/backend/commit/16af7faff12b4bffc584e95a2e91bdb41bff7eac))
* add semantic release workflow ([c140e62](https://github.com/see-paw/backend/commit/c140e62b5d34784c300c03d7cced8db633441fa2))
* Enable fetching tags in release workflowc ([cc434b7](https://github.com/see-paw/backend/commit/cc434b739085310b151a793b8f1c9980d27e5125))
* Initialized project ([7814e2e](https://github.com/see-paw/backend/commit/7814e2e0f156e1d7ee7c4ef8d928a0fd6ddbf4dc))
* **release:** 1.0.0 [skip ci] ([9e1ddbf](https://github.com/see-paw/backend/commit/9e1ddbfa8f2d1ee70ffefe049964bb0972f593bd))
* **release:** 1.0.0 [skip ci] ([81cfc4e](https://github.com/see-paw/backend/commit/81cfc4e85b9e3681fd8f2dfdf552611dfe24890f))
* **release:** 1.0.0 [skip ci] ([c827aec](https://github.com/see-paw/backend/commit/c827aec959a09d7302c6167e62fd14430e6d3fec))
* update .gitignore to ignore coverage report folder ([58af31c](https://github.com/see-paw/backend/commit/58af31c420a7335f10f71a15db04b26d46350614))

## 1.0.0 (2025-10-31)


### Chores

* Adapted new Database schema ([c879463](https://github.com/see-paw/backend/commit/c87946340d4c31d99e2af7949add04b681eec2a3))
* Add .releaserc.json for semantic release configuration ([0612143](https://github.com/see-paw/backend/commit/061214328be536d061a7a084d11a7236eb8417b5))
* add azure configuration for CD ([16af7fa](https://github.com/see-paw/backend/commit/16af7faff12b4bffc584e95a2e91bdb41bff7eac))
* add semantic release workflow ([c140e62](https://github.com/see-paw/backend/commit/c140e62b5d34784c300c03d7cced8db633441fa2))
* Initialized project ([7814e2e](https://github.com/see-paw/backend/commit/7814e2e0f156e1d7ee7c4ef8d928a0fd6ddbf4dc))
* **release:** 1.0.0 [skip ci] ([81cfc4e](https://github.com/see-paw/backend/commit/81cfc4e85b9e3681fd8f2dfdf552611dfe24890f))
* **release:** 1.0.0 [skip ci] ([c827aec](https://github.com/see-paw/backend/commit/c827aec959a09d7302c6167e62fd14430e6d3fec))
* update .gitignore to ignore coverage report folder ([58af31c](https://github.com/see-paw/backend/commit/58af31c420a7335f10f71a15db04b26d46350614))

## 1.0.0 (2025-10-31)


### Chores

* Adapted new Database schema ([c879463](https://github.com/see-paw/backend/commit/c87946340d4c31d99e2af7949add04b681eec2a3))
* Add .releaserc.json for semantic release configuration ([0612143](https://github.com/see-paw/backend/commit/061214328be536d061a7a084d11a7236eb8417b5))
* add azure configuration for CD ([16af7fa](https://github.com/see-paw/backend/commit/16af7faff12b4bffc584e95a2e91bdb41bff7eac))
* add semantic release workflow ([c140e62](https://github.com/see-paw/backend/commit/c140e62b5d34784c300c03d7cced8db633441fa2))
* Initialized project ([7814e2e](https://github.com/see-paw/backend/commit/7814e2e0f156e1d7ee7c4ef8d928a0fd6ddbf4dc))
* **release:** 1.0.0 [skip ci] ([c827aec](https://github.com/see-paw/backend/commit/c827aec959a09d7302c6167e62fd14430e6d3fec))

## 1.0.0 (2025-10-31)


### Chores

* Adapted new Database schema ([c879463](https://github.com/see-paw/backend/commit/c87946340d4c31d99e2af7949add04b681eec2a3))
* Add .releaserc.json for semantic release configuration ([0612143](https://github.com/see-paw/backend/commit/061214328be536d061a7a084d11a7236eb8417b5))
* add semantic release workflow ([c140e62](https://github.com/see-paw/backend/commit/c140e62b5d34784c300c03d7cced8db633441fa2))
* Initialized project ([7814e2e](https://github.com/see-paw/backend/commit/7814e2e0f156e1d7ee7c4ef8d928a0fd6ddbf4dc))

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
