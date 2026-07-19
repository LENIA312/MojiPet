pipeline {
    agent any

    parameters {
        string(name: 'DEVICE_UDID', defaultValue: '00008150-00084DD80E84401C', description: 'UDID of the paired iOS device to install the build onto (Xcode > Window > Devices and Simulators). Leave blank to skip device install.')
        string(name: 'TEAM_ID', defaultValue: 'W75NLV9SPT', description: 'Apple Personal Team ID (Xcode > Settings > Accounts > Manage Certificates)')
        string(name: 'PROVISIONING_PROFILE_UUID', defaultValue: 'd4d68a16-e44d-435c-8e38-a8942c9e617f', description: 'UUID of the locally-generated provisioning profile for com.lenia.mojipet (find via docs/ios_ci_setup.md). Regenerate by doing a manual Xcode Run when this expires or a new device is added.')
    }

    environment {
        UNITY_PATH = '/Applications/Unity/Hub/Editor/6000.4.9f1/Unity.app/Contents/MacOS/Unity'
        BUILD_OUTPUT = "${WORKSPACE}/Builds/iOS"
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Unity iOS Export') {
            steps {
                sh '''
                    "$UNITY_PATH" \
                        -quit -batchmode -nographics \
                        -projectPath "$WORKSPACE" \
                        -executeMethod Mojipet.Editor.CI.BuildScript.BuildIos \
                        -buildOutput "$BUILD_OUTPUT" \
                        -teamId "$TEAM_ID" \
                        -logFile "$WORKSPACE/unity_build.log"
                '''
            }
        }

        stage('Xcode Build & Sign') {
            steps {
                sh 'chmod +x ci/build_ios.sh'
                sh "ci/build_ios.sh \"$BUILD_OUTPUT\" \"${params.TEAM_ID}\" \"${params.PROVISIONING_PROFILE_UUID}\""
            }
        }

        stage('Install to Device') {
            when {
                expression { return params.DEVICE_UDID?.trim() }
            }
            steps {
                sh 'chmod +x ci/install_ios.sh'
                sh "ci/install_ios.sh \"$BUILD_OUTPUT/build/Mojipet.xcarchive\" \"${params.DEVICE_UDID}\""
            }
        }
    }

    post {
        always {
            archiveArtifacts artifacts: 'unity_build.log', allowEmptyArchive: true
        }
    }
}
