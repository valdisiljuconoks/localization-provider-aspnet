; (function () {
    angular.module('resourceUIApp', ['ui.bootstrap'])
    .controller('resourcesController', ['$scope', '$uibModal', '$http',
            function ($scope, $uibModal, $http) {

                $scope.getTranslation = function (resource, language) {
                    var translation = null;
                    angular.forEach(resource.Value, function (res) {
                        if (res.SourceCulture.Code == language.Code && res.Value.length > 0)
                            translation = res.Value;
                    });

                    return translation;
                };

                var vm = this;

                vm.resources = undefined;
                vm.languages = undefined;
                vm.adminMode = undefined;
                vm.disableRemoveTranslationButton = undefined;
                vm.hideDeleteButton = undefined;

                vm.fetch = fetch;

                vm.open = function (resource, lang) {

                    var selectedResource = resource,
                        selectedLanguage = lang,
                        disableRemoveTranslation = vm.disableRemoveTranslationButton;

                    var modalInstance = $uibModal.open({
                        templateUrl: 'popup-content.html',
                        controller: 'ModalInstanceCtrl',
                        size: 'lg',
                        resolve: {
                            resource: function () { return selectedResource; },
                            translation: function() {
                                var translation = $scope.getTranslation(selectedResource, selectedLanguage);
                                return translation === "N/A" ? "" : translation;
                            },
                            disableRemoveButton: function() { return disableRemoveTranslation; }
                        }
                    });

                    modalInstance.result.then(
                        function (result) {
                            if (result.event === 'ok') {
                                $http.post('api/update',
                                        {
                                            key: selectedResource.Key,
                                            language: selectedLanguage.Code,
                                            newTranslation: result.translation
                                        })
                                    .success(function() {
                                        // TODO: show notification
                                        vm.fetch();
                                    });
                            }
                            else if (result.event === 'remove') {
                                $http.post('api/remove', { key: selectedResource.Key, language: selectedLanguage.Code })
                                     .success(function() {
                                        // TODO: show notification
                                        vm.fetch();
                                     });
                            }
                        });
                };


                function fetch() {
                    $http.get('api/get')
                        .success(function (data) {
                            try {
                                var response = angular.fromJson(data);
                                vm.resources = response.Resources;
                                vm.languages = response.Languages;
                                vm.adminMode = response.AdminMode;
                                vm.disableRemoveTranslationButton = response.IsRemoveTranslationButtonDisabled;
                                vm.hideDeleteButton = response.HideDeleteButton;
                            } catch (e) {
                                // error may occur when service returns html for login page instead of json (unauthorized access, session expired, etc)
                                alert(e);
                            }
                        });
                }
            }
    ]);


    angular.module('resourceUIApp').controller('ModalInstanceCtrl', function ($scope, $uibModalInstance, resource, translation, disableRemoveButton) {

        $scope.resource = resource;
        $scope.translation = translation;
        $scope.disableRemoveButton = disableRemoveButton;

        $scope.ok = function () {
            $uibModalInstance.close({ translation: $scope.translation, event: 'ok' });
        };

        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };

        $scope.remove = function () {
            if (confirm('Do you want to remove translation?')) {
                $uibModalInstance.close({ event: 'remove' });
            }
        };
    });
})();
