pipeline {

agent {
            label 'memphis-jenkins-big-fleet,'
    }

    stages {
        stage('Install .NET SDK') {
            steps {
                script {
                    def branchName = env.BRANCH_NAME ?: ''
                    // Check if the branch is 'latest'
                    if (branchName == 'latest') {
                        // Read version from version-beta.conf
                        def version = readFile('version.conf').trim()
                        // Set the VERSION environment variable to the version from the file
                        env.versionTag = version
                        echo "Using version from version-beta.conf: ${env.VERSION}"
                    } else {
                        def version = readFile('version-beta.conf').trim()
                        env.versionTag = version
                        echo "Using version from version-beta.conf: ${env.versionTag}"                        
                    }
                    // Define variables for convenience
                    def dotnetVersion = '8.0'
                    def installScriptUrl = 'https://dot.net/v1/dotnet-install.sh'

                    // Download the dotnet-install.sh script
                    sh "curl -Lsfo dotnet-install.sh ${installScriptUrl}"

                    // Make the script executable
                    sh "chmod +x dotnet-install.sh"

                    // Run the script to install the .NET SDK
                    sh "./dotnet-install.sh --channel ${dotnetVersion} --version latest"

                    // Optionally, add dotnet to PATH. Adjust the path if you've used a custom installation directory
                    sh 'echo "export PATH=\$PATH:\$HOME/.dotnet" >> $HOME/.bashrc'
                    sh 'source $HOME/.bashrc'                    
                }            
                // sh """
                //   wget https://dot.net/v1/dotnet-install.sh
                //   chmod +x dotnet-install.sh
                //   ./dotnet-install.sh -c 8.0 -InstallDir ~/dotnet
                // """
                sh """
                dotnet --list-sdks
                dotnet --list-runtimes
                """
            }
        }

        stage('Build project') {
            steps {
              sh """
                      ~/.dotnet/dotnet build -c Release superstream.sln
                    """
            }
        }

        // stage('Package the project') {
        //     steps {
        //       sh """
        //         ~/.dotnet/dotnet pack -v normal -c Release --no-restore --include-source /p:ContinuousIntegrationBuild=true -p:PackageVersion=$versionTag src/Superstream/Superstream.csproj
        //       """
        //     }
        // }

        // stage('Publish to NuGet') {
        //     steps {
        //       withCredentials([string(credentialsId: 'NUGET_KEY', variable: 'NUGET_KEY')]) {
        //         sh """
        //           ~/.dotnet/dotnet nuget push ./src/Superstream/bin/Release/Superstream.${versionTag}.nupkg --source https://api.nuget.org/v3/index.json --api-key $NUGET_KEY
        //         """
        //       }
        //     }
        // }

      stage('Create new Release'){
            when {
                expression { env.BRANCH_NAME == 'latest' }
            }        
        steps {
          sh """
            sudo dnf config-manager --add-repo https://cli.github.com/packages/rpm/gh-cli.repo -y
            sudo dnf install gh -y
            sudo dnf install jq -y
          """
              withCredentials([sshUserPrivateKey(keyFileVariable:'check',credentialsId: 'main-github')]) {
                sh """
                  git reset --hard origin/latest
                  GIT_SSH_COMMAND='ssh -i $check' git checkout -b \$(cat version.conf)
                  GIT_SSH_COMMAND='ssh -i $check' git push --set-upstream origin \$(cat version.conf)
                """
              }
              withCredentials([string(credentialsId: 'gh_token', variable: 'GH_TOKEN')]) {
                sh(script:"""gh release create \$(cat version.conf) --generate-notes""", returnStdout: true)
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
    emailext (
        subject: "SUCCESSFUL: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]'",
        body: """SUCCESSFUL: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]':
        Check console output and connection attributes at ${env.BUILD_URL}""",
        recipientProviders: [requestor()]
    )
}
def notifyFailed() {
    emailext (
        subject: "FAILED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]'",
        body: """FAILED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]':
        Check console output at ${env.BUILD_URL}""",
        recipientProviders: [requestor()]
    )
}
