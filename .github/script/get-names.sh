#!/usr/bin/env bash
# IF THE WORKFLOW FAILS BECAUSE OF PERMISSION USE THIS COMMAND IN GIT SHELL: 'git update-index --chmod=+x ./.github/script/get-project-name.sh'
# REPO_NAME contains the github.repository value which is owner/repo-name.
echo "${REPO_NAME}"
# Split by / and taking the element at index 1 (owner/repo-name -> repo-name)
readarray -d "/" -t splitted <<< "${REPO_NAME}"  
repoName="${splitted[1]}"
# By guidelines, repo name are written with a dash separator between context and underscore to mark spaces. 
# I.E. client_name-very_long_repo_name-another_context -> This should return VeryLonRepoName.
# So start by trying splitting the repo-name by dash (-)
readarray -d "-" -t splitted <<< "$repoName"
# Then get the element at index 1 (if it exists) otherwise take the 0.
min=$(( ${#splitted[@]} > 1 ? 1 : 0 ))
# client_name-repo_name -> repo_name; messyreponame -> messyreponame
projectName="${splitted[${min}]}"

echo $projectName
# Register the formatted project name to the GITHUB_OUTPUT's project_name variable. 
# This output is then used inside the job 'outputs' to register a variable.
echo "name=${projectName}" >> $GITHUB_OUTPUT 

if [[ "$min" == 1 ]]; then
    readarray -d "-" -t splitted <<< "$repoName"
    clientName="${splitted[0]}"
    echo $clientName
    echo "client=${clientName}" >> $GITHUB_OUTPUT
fi