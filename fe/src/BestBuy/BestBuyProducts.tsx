import {ChangeEvent, Dispatch, Fragment, SetStateAction, useEffect, useState} from "react";
import "./BestBuyProducts.css"

type Product = {
    sku: number,
    name: string,
    price: number,
    thumbnailImage: undefined | string
}

type UserOptions = {
    sorts: {
        sortPriceAscending: boolean | undefined,
        sortNameAscending: boolean | undefined
    },
    filters: {
        name: string | undefined,
        types: string[] | undefined
    }

}

const initialUserOptions: UserOptions = {
    sorts: {
        sortPriceAscending: undefined,
        sortNameAscending: undefined,
    },
    filters: {
        name: undefined,
        types: []
    }
}

function Component() {
    const [products, setProducts] = useState<undefined | Product[]>(undefined);
    const [loading, setLoading] = useState(true);
    const userFiltersAndSorts = useState<UserOptions>(initialUserOptions)
    const [userOptions, setUserOptions] = userFiltersAndSorts;

    useEffect(() => {

        const nameFilter = userOptions.filters.name === undefined
            ? ""
            : `name=${userOptions.filters.name}&`;

        console.log(userOptions.filters.types)
        const typesFilter = userOptions.filters.types === undefined
            ? ""
            : userOptions.filters.types.reduce((acc, type) => acc + `type=${type}&`, "")

        const priceSort = userOptions.sorts.sortPriceAscending === undefined
            ? ""
            : `sortPriceAscending=${userOptions.sorts.sortPriceAscending}&`

        const nameSort = userOptions.sorts.sortNameAscending === undefined
            ? ""
            : `sortNameAscending=${userOptions.sorts.sortNameAscending}&`

        const uri = `http://localhost:5121/products?${nameFilter}${typesFilter}${priceSort}${nameSort}`;
        console.log(uri);
        fetch(uri)
            .then(res => res.json())
            .then(p => {
                setProducts(p);
                setLoading(false);
            })
    }, [userOptions]);

    return (<div className="List">
        {
            loading
                ? "loading..."
                : <>
                    <FiltersAndSorts userFiltersAndSorts={userFiltersAndSorts}/>
                    {products
                        ?.map((product, idx) => {
                            const key = `product-key-${idx}`;
                            return (<ProductLine key={key} product={product}/>);
                        })
                    }
                </>

        }
    </div>);
}

type FiltersAndSortsProps = {
    userFiltersAndSorts: [UserOptions, Dispatch<SetStateAction<UserOptions>>]

}

function FiltersAndSorts(
    {
        userFiltersAndSorts
    }: FiltersAndSortsProps) {


    const [knownTypes, setKnownTypes] = useState<string[]>([]);
    const [userOptions, setUserOptions] = userFiltersAndSorts;

    useEffect(() => {
        fetch("http://localhost:5121/products/types")
            .then(res => res.json())
            .then(t => setKnownTypes(t))
    }, [userOptions]);

    function onTypeChange(e: ChangeEvent<HTMLInputElement>) {
        setUserOptions(
            prevState => {
                const newTypes =
                    e.target.checked
                        ? prevState.filters.types?.concat([e.target.value])
                        : prevState.filters.types?.filter(v => v !== e.target.value);

                return {
                    ...prevState,
                    filters: {
                        ...prevState.filters,
                        types: newTypes
                    }
                }
            }
        );
    }

    let typingTimer: NodeJS.Timeout | undefined;

    function onNameChange(e: ChangeEvent<HTMLInputElement>) {
        const searchTerm = e.target.value;

        if (typingTimer) {
            clearTimeout(typingTimer);
        }

        typingTimer = setTimeout(() => {
            console.log(`Search term: ${searchTerm}`);
            setUserOptions(prev => {
                return {
                    ...prev,
                    filters: {
                        ...prev.filters,
                        name: searchTerm
                    }
                }
            });

            typingTimer = undefined;
        }, 500);
    }

    function onPriceSortChange(e: React.ChangeEvent<HTMLSelectElement>) {
        console.log(e.target.value);
        console.log(e.target.value == undefined);
        console.log(e.target.value === undefined);
        setUserOptions(prev => {
            return {
                ...prev,
                sorts: {
                    ...prev.sorts,
                    sortPriceAscending: e.target.value === ""
                        ? undefined
                        : JSON.parse(e.target.value)
                }
            }
        });
    }

    function onNameSortChange(e: React.ChangeEvent<HTMLSelectElement>) {
        setUserOptions(prev => {
            return {
                ...prev,
                sorts: {
                    ...prev.sorts,
                    sortNameAscending: e.target.value === ""
                        ? undefined
                        : JSON.parse(e.target.value)
                }
            }
        });
    }

    return (
        <>
            <form className="TypeFilter">
                <div>Filter by Type</div>
                <div>
                    {knownTypes.map((option, idx) => {
                            const key = `type-option-${idx}`;
                            return (
                                <Fragment key={key}>
                                    <span>{option}</span>
                                    <input type="checkbox"
                                           value={option}
                                           onChange={e => onTypeChange(e)}
                                    />
                                </Fragment>);
                        }
                    )}
                </div>
            </form>
            <form className="NameFilter">
                <div>Filter by Name</div>
                <div>
                    <input type="text" onChange={e => onNameChange(e)}/>
                </div>
            </form>
            <div className="Sorts">
                <form className="PriceSort">
                    <div>Sort by price</div>
                    <div>
                        <select value={undefined} onChange={e => onPriceSortChange(e)}>
                            <option value={""}>Default</option>
                            <option value={`${true}`}>Ascending</option>
                            <option value={`${false}`}>Descending</option>
                        </select>
                    </div>
                </form>
                <form className="NameSort">
                    <div>Sort by name</div>
                    <div>
                        <select value={undefined} onChange={e => onNameSortChange(e)}>
                            <option value={""}>Default</option>
                            <option value={`${true}`}>Ascending</option>
                            <option value={`${false}`}>Descending</option>
                        </select>
                    </div>
                </form>
            </div>
        </>
    );
}

type ProductLineProps = {
    product: Product
}

function ProductLine(
    {
        product
    }: ProductLineProps) {
    return (
        <div className="Line">
            <div>
                <img src={product.thumbnailImage} alt={product.thumbnailImage}/>
            </div>
            <ul>
                <li>{product.sku}</li>
                <li>{product.name}</li>
                <li>$ {product.price}</li>
            </ul>
        </div>);
}

export default Component;