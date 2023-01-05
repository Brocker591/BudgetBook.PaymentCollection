# BudgetBook.PaymentCollection


## Build the docker image local
```powershell
$version="1.0.1"


docker build -t "brocker591/budgetbook.paymentcollection:$version"  .

```

## Run the docker image
```powershell

docker run -it --rm -p 5100:5100 -e MongoDbSettings__Host=mongo --name paymentcollection --network playinfra_default brocker591/budgetbook.paymentcollection:$version
```

## Run the docker image with CosmosDB and Service Bus
```powershell

$cosmosDbConnString="[CONN STRING HERE]"


docker run -it --rm -p 5100:5100 --name paymentcollection -e MongoDbSettings__ConnectionString=$cosmosDbConnString brocker591/budgetbook.paymentcollection:$version
```

## Publishing the Docker image
```powershell

docker push "brocker591/budgetbook.paymentcollection:$version"
```