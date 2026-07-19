pipeline {
    agent any

    parameters {
        string(name: 'DEVICE_UDID', defaultValue: '', description: 'UDID of the paired iOS device to install the build onto (Xcode > Window > Devices and Simulators)')
        string(name: 'TEAM_ID', defaultValue: '', description: 'Apple Personal Team ID (Xcode > Settings > Accounts > Manage Certificates)')
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
                sh "ci/build_ios.sh \"$BUILD_OUTPUT\" \"${params.TEAM_ID}\""
            }
        }

        stage('Install to Device') {
            when {
                expression { return params.DEVICE_UDID?.trim() }
            }
            steps {
                sh 'chmod +x ci/install_ios.sh'
                sh "ci/install_ios.sh \"$BUILD_OUTPUT/build/export\" \"${params.DEVICE_UDID}\""
            }
        }
    }

    post {
        always {
            archiveArtifacts artifacts: 'unity_build.log', allowEmptyArchive: true
        }
        success {
            archiveArtifacts artifacts: 'Builds/iOS/build/export/*.ipa', allowEmptyArchive: true
        }
    }
}
