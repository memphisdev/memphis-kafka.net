pipeline {
    agent {
        label 'memphis-jenkins-small-fleet-agent'
    }
    environment {
        DOTNET_ROOT = "$HOME/.dotnet"
        PATH = "$DOTNET_ROOT:$PATH"
    }    

    stages {
        stage('Install .NET SDK') {
            steps {
                script {
                    def versionFile = (env.BRANCH_NAME == 'latest') ? 'version.conf' : 'version-beta.conf'
                    env.versionTag = readFile(versionFile).trim()
                    echo "Using version from ${versionFile}: ${env.versionTag}"
                }
                sh """
                    wget https://dot.net/v1/dotnet-install.sh
                    chmod +x dotnet-install.sh
                    ./dotnet-install.sh -c LTS
                """
            }
        }

        stage('Build project') {
            steps {
                sh "${DOTNET_ROOT}/dotnet build -c Release superstream.sln"
            }
        }

        stage('Package the project') {
            steps {
                sh """
                    ${DOTNET_ROOT}/dotnet pack -v normal -c Release --no-restore --include-source /p:ContinuousIntegrationBuild=true -p:PackageVersion=$versionTag src/Superstream/Superstream.csproj
                """
            }
        }

        stage('Publish to NuGet') {
            steps {
                withCredentials([string(credentialsId: 'NUGET_KEY', variable: 'NUGET_KEY')]) {
                    sh """
                        ${DOTNET_ROOT}/dotnet nuget push ./src/Superstream/bin/Release/Superstream.${versionTag}.nupkg --source https://api.nuget.org/v3/index.json --api-key $NUGET_KEY
                    """
                }
            }
        }

        stage('Create new Release') {
            when {
                expression { env.BRANCH_NAME == 'latest' }
            }
            steps {
                sh """
                    sudo dnf config-manager --add-repo https://cli.github.com/packages/rpm/gh-cli.repo -y
                    sudo dnf install gh -y
                    sudo dnf install jq -y
                """
                withCredentials([sshUserPrivateKey(credentialsId: 'main-github', keyFileVariable: 'check')]) {
                    sh """
                        git reset --hard origin/latest
                        GIT_SSH_COMMAND='ssh -i $check' git checkout -b \$(cat version.conf)
                        GIT_SSH_COMMAND='ssh -i $check' git push --set-upstream origin \$(cat version.conf)
                    """
                }
                withCredentials([string(credentialsId: 'gh_token', variable: 'GH_TOKEN')]) {
                    sh(script: "gh release create \$(cat version.conf) --generate-notes", returnStdout: true)
                }
            }
        }        
    }

    post {
        always {
            cleanWs()
        }
        success {
            notifySuccessful()
        }
        failure {
            notifyFailed()
        }
    }
}

def notifySuccessful() {
    emailext(
        subject: "SUCCESSFUL: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]'",
        body: """SUCCESSFUL: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]':
        Check console output and connection attributes at ${env.BUILD_URL}""",
        recipientProviders: [requestor()]
    )
}

def notifyFailed() {
    emailext(
        subject: "FAILED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]'",
        body: """FAILED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]':
        Check console output at ${env.BUILD_URL}""",
        recipientProviders: [requestor()]
    )
}
